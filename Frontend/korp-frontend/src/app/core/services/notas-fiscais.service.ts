import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotaFiscal } from '../models/nota-fiscal.model';

export interface CriarNotaFiscalRequest {
  numero: string;
  dataEmissao: string;
  itens: {
    codigoProduto: string;
    descricaoProduto: string;
    quantidade: number;
  }[];
}

@Injectable({
  providedIn: 'root'
})
export class NotasFiscaisService {
  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:7240/api/notasfiscais';

  listarTodos(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.apiUrl);
  }

  buscarPorId(id: string): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.apiUrl}/${id}`);
  }

  criar(payload: CriarNotaFiscalRequest): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.apiUrl, payload);
  }

  fechar(id: string): Observable<NotaFiscal> {
    return this.http.put<NotaFiscal>(`${this.apiUrl}/${id}/fechar`, {});
  }
}