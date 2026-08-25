import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { NotaFiscal } from '../models/NotaFiscal';

export interface NovoItemNotaFiscal {
  produtoId: number;
  quantidade: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotaFiscalService {

  private apiUrl = 'http://localhost:5274/api/NotasFiscais';

  constructor(private http: HttpClient) {}

  listar(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.apiUrl);
  }

  obterPorId(id: number): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.apiUrl}/${id}`);
  }

  criar(itens: NovoItemNotaFiscal[]): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.apiUrl, itens);
  }

  fechar(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/fechar`, {});
  }

  processar(id: number) {
    return this.http.post(
      `${this.apiUrl}/${id}/processar`,
      {}
    );
  }
}