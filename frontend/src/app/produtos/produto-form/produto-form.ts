import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProdutoService } from '../../services/produto';

@Component({
  selector: 'app-produto-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './produto-form.html',
  styleUrl: './produto-form.css'
})
export class ProdutoFormComponent implements OnInit {
  form: FormGroup;
  modoEdicao = signal(false);
  produtoId: number | null = null;
  salvando = signal(false);
  erro = signal('');

  constructor(
    private fb: FormBuilder,
    private produtoService: ProdutoService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      codigo: ['', Validators.required],
      descricao: ['', Validators.required],
      saldo: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.modoEdicao.set(true);
      this.produtoId = Number(idParam);
      this.carregarProduto(this.produtoId);
    }
  }

  carregarProduto(id: number): void {
    this.produtoService.obterPorId(id).subscribe({
      next: (produto) => {
        this.form.patchValue(produto);
      },
      error: (err) => {
        this.erro.set('Não foi possível carregar o produto.');
        console.error(err);
      }
    });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.salvando.set(true);
    this.erro.set('');

    const produto = this.form.value;

    const requisicao = this.modoEdicao()
      ? this.produtoService.atualizar(this.produtoId!, produto)
      : this.produtoService.cadastrar(produto);

    requisicao.subscribe({
      next: () => {
        this.salvando.set(false);
        this.router.navigate(['/produtos']);
      },
      error: (err) => {
        this.salvando.set(false);
        this.erro.set(err.error ?? 'Não foi possível salvar o produto.');
        console.error(err);
      }
    });
  }
}