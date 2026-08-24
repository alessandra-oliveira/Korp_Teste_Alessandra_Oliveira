import { Component, OnInit, signal, computed } from '@angular/core';
import { Router } from '@angular/router';

import { NotaFiscalService, NovoItemNotaFiscal } from '../../../shared/services/nota-fiscal';
import { ProdutoService } from '../../../shared/services/produto';
import { Produto } from '../../../shared/models/ProdutoModel';

interface LinhaItem {
  produtoId: number | null;
  quantidade: number;
}

@Component({
  selector: 'app-nova-nota-fiscal',
  imports: [],
  templateUrl: './nova-nota-fiscal.html',
  styleUrl: './nova-nota-fiscal.css',
})
export class NovaNotaFiscalComponent implements OnInit {
  produtos = signal<Produto[]>([]);
  linhas = signal<LinhaItem[]>([{ produtoId: null, quantidade: 1 }]);
  salvando = signal(false);
  erro = signal('');
  carregandoProdutos = signal(false);

  podeSalvar = computed(
    () =>
      this.linhas().length > 0 &&
      this.linhas().every((l) => l.produtoId !== null && l.quantidade > 0) &&
      !this.temLinhaExcedendoSaldo(),
  );

  temLinhaExcedendoSaldo = computed(() => this.linhas().some((l) => this.quantidadeExcedeSaldo(l)));

  constructor(
    private notaFiscalService: NotaFiscalService,
    private produtoService: ProdutoService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.carregandoProdutos.set(true);
    this.produtoService.listar().subscribe({
      next: (produtos) => {
        this.produtos.set(produtos);
        this.carregandoProdutos.set(false);
      },
      error: () => {
        this.erro.set('Não foi possível carregar os produtos.');
        this.carregandoProdutos.set(false);
      },
    });
  }

  adicionarLinha(): void {
    this.linhas.update((linhas) => [...linhas, { produtoId: null, quantidade: 1 }]);
  }

  removerLinha(index: number): void {
    this.linhas.update((linhas) => linhas.filter((_, i) => i !== index));
  }

  atualizarProduto(index: number, produtoId: string): void {
    this.linhas.update((linhas) => {
      const copia = [...linhas];
      copia[index] = { ...copia[index], produtoId: produtoId ? Number(produtoId) : null };
      return copia;
    });
  }

  atualizarQuantidade(index: number, quantidade: string): void {
    this.linhas.update((linhas) => {
      const copia = [...linhas];
      copia[index] = { ...copia[index], quantidade: Number(quantidade) };
      return copia;
    });
  }

  saldoDisponivel(produtoId: number | null): number | null {
    if (produtoId === null) return null;
    const produto = this.produtos().find((p) => p.id === produtoId);
    return produto ? produto.saldo : null;
  }

  quantidadeExcedeSaldo(linha: LinhaItem): boolean {
    const saldo = this.saldoDisponivel(linha.produtoId);
    return saldo !== null && linha.quantidade > saldo;
  }

  salvar(): void {
    if (!this.podeSalvar()) return;

    this.salvando.set(true);
    this.erro.set('');

    const itens: NovoItemNotaFiscal[] = this.linhas().map((l) => ({
      produtoId: l.produtoId!,
      quantidade: l.quantidade,
    }));

    this.notaFiscalService.criar(itens).subscribe({
      next: (nota) => {
        this.salvando.set(false);
        this.router.navigate(['/notas-fiscais', nota.id]);
      },
      error: () => {
        this.salvando.set(false);
        this.erro.set(
          'Não foi possível criar a nota fiscal. Verifique os dados e tente novamente.',
        );
      },
    });
  }

  cancelar(): void {
    this.router.navigate(['/notas-fiscais']);
  }
}
