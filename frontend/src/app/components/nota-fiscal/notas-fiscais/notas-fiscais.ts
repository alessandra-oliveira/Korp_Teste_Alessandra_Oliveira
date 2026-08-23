import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { NotaFiscal } from '../../models/NotaFiscal';
import { NotaFiscalService } from '../../services/nota-fiscal';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-notas-fiscais',
  templateUrl: './notas-fiscais.html',
  styleUrl: './notas-fiscais.css',
  imports: [DatePipe],
})
export class ListaNotasFiscaisComponent implements OnInit {
  notas = signal<NotaFiscal[]>([]);
  carregando = signal(false);
  erro = signal('');

  constructor(
    private notaFiscalService: NotaFiscalService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.carregarNotas();
  }

  carregarNotas(): void {
    this.carregando.set(true);
    this.erro.set('');

    this.notaFiscalService.listar().subscribe({
      next: (notas) => {
        this.notas.set(notas);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Não foi possível carregar as notas fiscais.');
        this.carregando.set(false);
      },
    });
  }

  novaNota(): void {
    this.router.navigate(['/notas-fiscais/nova']);
  }

  visualizar(id: number): void {
    this.router.navigate(['/notas-fiscais', id]);
  }
}
