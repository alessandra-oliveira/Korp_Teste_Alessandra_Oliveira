import { Component, OnInit, signal } from '@angular/core';
import { ProdutoService } from '../../services/produto';
import { Produto } from '../../models/ProdutoModel';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-produto-list',
  imports: [RouterLink],
  templateUrl: './produto-list.html',
  styleUrl: './produto-list.css'
})
export class ProdutoListComponent implements OnInit {
  produtos = signal<Produto[]>([]);
  carregando = signal(false);
  erro = signal('');

  constructor(private produtoService: ProdutoService) { }

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.carregando.set(true);
    this.erro.set('');

    this.produtoService.listar().subscribe({
      next: (produtos) => {
        this.produtos.set(produtos);
        this.carregando.set(false);
      },
      error: (err) => {
        this.erro.set('Não foi possível carregar os produtos. Verifique se a API do Estoque está rodando.');
        this.carregando.set(false);
        console.error(err);
      }
    });
  }

  excluirProduto(id: number): void {
    if (!confirm('Tem certeza que deseja excluir este produto?')) {
      return;
    }

    this.produtoService.excluir(id).subscribe({
      next: () => {
        this.carregarProdutos();
      },
      error: (err) => {
        this.erro.set('Não foi possível excluir o produto.');
        console.error(err);
      }
    });
  }
}