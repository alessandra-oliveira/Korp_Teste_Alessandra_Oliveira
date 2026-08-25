import { Component, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { NotaFiscal } from '../../../shared/models/NotaFiscal';
import { NotaFiscalService } from '../../../shared/services/nota-fiscal';
import { ProdutoService } from '../../../shared/services/produto';
import { Produto } from '../../../shared/models/ProdutoModel';

interface ItemComProduto {
  produtoId: number;
  descricaoProduto: string;
  codigoProduto: string;
  quantidade: number;
  produtoEncontrado: boolean;
}

@Component({
  selector: 'app-detalhe-nota-fiscal',
  templateUrl: './detalhe-nota-fiscal.html',
  styleUrl: './detalhe-nota-fiscal.css',
  imports: [DatePipe],
})
export class DetalheNotaFiscalComponent implements OnInit {
  nota = signal<NotaFiscal | null>(null);
  itensComProduto = signal<ItemComProduto[]>([]);
  carregando = signal(false);
  erro = signal('');
  processando = signal(false);
  imprimindo = signal(false);

  podeImprimir = computed(() => this.nota()?.status === 'Processada');
  podeProcessar = computed(() => this.nota()?.status === 'Aberta');

  private notaId!: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private notaFiscalService: NotaFiscalService,
    private produtoService: ProdutoService,
  ) {}

  ngOnInit(): void {
    this.notaId = Number(this.route.snapshot.paramMap.get('id'));
    this.carregarNota();
  }

  carregarNota(): void {
    this.carregando.set(true);
    this.erro.set('');

    this.notaFiscalService.obterPorId(this.notaId).subscribe({
      next: (nota) => {
        this.nota.set(nota);
        this.carregarProdutosDosItens(nota);
      },
      error: () => {
        this.erro.set('Não foi possível carregar a nota fiscal.');
        this.carregando.set(false);
      },
    });
  }

  private carregarProdutosDosItens(nota: NotaFiscal): void {
    if (nota.itens.length === 0) {
      this.itensComProduto.set([]);
      this.carregando.set(false);
      return;
    }

    const chamadas = nota.itens.map((item) =>
      this.produtoService.obterPorId(item.produtoId).pipe(catchError(() => of(null))),
    );

    forkJoin(chamadas).subscribe({
      next: (produtos: (Produto | null)[]) => {
        const itens = nota.itens.map((item, index) => {
          const produto = produtos[index];

          return {
            produtoId: item.produtoId,
            descricaoProduto: produto?.descricao ?? 'Produto não encontrado (excluído)',
            codigoProduto: produto?.codigo ?? '—',
            quantidade: item.quantidade,
            produtoEncontrado: produto !== null,
          };
        });

        this.itensComProduto.set(itens);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Erro inesperado ao carregar os itens da nota.');
        this.carregando.set(false);
      },
    });
  }

  imprimir(): void {
    const notaAtual = this.nota();

    if (!notaAtual || notaAtual.status !== 'Processada' || this.imprimindo()) {
      return;
    }

    this.imprimindo.set(true);
    this.erro.set('');

    this.notaFiscalService.fechar(notaAtual.id).subscribe({
      next: () => {
        this.notaFiscalService.obterPorId(notaAtual.id).subscribe({
          next: (nota) => {
            this.nota.set(nota);
            this.imprimindo.set(false);

            if (nota.status === 'Fechada') {
              setTimeout(() => window.print(), 0);
            }
          },
          error: () => {
            this.imprimindo.set(false);
            this.erro.set('A nota foi fechada, mas não foi possível confirmar o status.');
          },
        });
      },
      error: (err) => {
        this.imprimindo.set(false);
        this.erro.set(err.error?.erro ?? 'Não foi possível fechar a nota fiscal.');
      },
    });
  }

  voltar(): void {
    this.router.navigate(['/notas-fiscais']);
  }

  processar(): void {
    const notaAtual = this.nota();

    if (!notaAtual || notaAtual.status !== 'Aberta' || this.processando()) {
      return;
    }

    this.processando.set(true);
    this.erro.set('');

    this.notaFiscalService.processar(notaAtual.id).subscribe({
      next: () => {
        this.aguardarProcessamento(notaAtual.id);
      },
      error: (err) => {
        this.processando.set(false);
        this.erro.set(
          err.error?.erro ?? 'Não foi possível processar a nota fiscal. Tente novamente.',
        );
      },
    });
  }

  private aguardarProcessamento(notaId: number, tentativas = 0): void {
    const MAX_TENTATIVAS = 120;

    this.notaFiscalService.obterPorId(notaId).subscribe({
      next: (nota) => {
        this.nota.set(nota);

        if (nota.status === 'Processada') {
          this.processando.set(false);
        } else if (tentativas < MAX_TENTATIVAS) {
          setTimeout(() => this.aguardarProcessamento(notaId, tentativas + 1), 1000);
        } else {
          this.processando.set(false);
          this.erro.set(
            'O processamento está demorando mais que o esperado. ' +
              'Verifique o status novamente em instantes.',
          );
        }
      },

      error: () => {
        this.processando.set(false);
        this.erro.set('Não foi possível verificar o status da nota fiscal.');
      },
    });
  }
}
