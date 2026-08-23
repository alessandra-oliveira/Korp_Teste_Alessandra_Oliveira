import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { NotaFiscal } from '../../models/NotaFiscal';
import { NotaFiscalService } from '../../services/nota-fiscal';
import { ProdutoService } from '../../services/produto';
import { Produto } from '../../models/ProdutoModel';

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
  imports: [DatePipe]
})
export class DetalheNotaFiscal implements OnInit {

  nota = signal<NotaFiscal | null>(null);
  itensComProduto = signal<ItemComProduto[]>([]);
  carregando = signal(false);
  erro = signal('');
  fechando = signal(false);

  private notaId!: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private notaFiscalService: NotaFiscalService,
    private produtoService: ProdutoService
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
      }
    });
  }

  private carregarProdutosDosItens(nota: NotaFiscal): void {
    if (nota.itens.length === 0) {
      this.itensComProduto.set([]);
      this.carregando.set(false);
      return;
    }

    const chamadas = nota.itens.map(item =>
      this.produtoService.obterPorId(item.produtoId).pipe(
        catchError(() => of(null))
      )
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
            produtoEncontrado: produto !== null
          };
        });

        this.itensComProduto.set(itens);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Erro inesperado ao carregar os itens da nota.');
        this.carregando.set(false);
      }
    });
  }

  fecharNota(): void {
    const notaAtual = this.nota();
    if (!notaAtual) return;

    this.fechando.set(true);
    this.erro.set('');

    this.notaFiscalService.fechar(notaAtual.id).subscribe({
      next: () => {
        this.fechando.set(false);
        this.carregarNota();
      },
      error: () => {
        this.fechando.set(false);
        this.erro.set('Não foi possível fechar a nota fiscal. Tente novamente.');
      }
    });
  }

  imprimir(): void {
    window.print();
  }

  voltar(): void {
    this.router.navigate(['/notas-fiscais']);
  }
}