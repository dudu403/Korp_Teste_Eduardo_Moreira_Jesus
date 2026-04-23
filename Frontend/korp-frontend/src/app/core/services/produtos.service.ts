import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Produto } from '../models/produto.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class ProdutosService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7093/api/produtos';

  listarPaginado(page: number, pageSize: number, search: string = ''): Observable<PagedResult<Produto>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedResult<Produto>>(this.apiUrl, { params });
  }

  listarTodos(): Observable<Produto[]> {
    return this.http
      .get<PagedResult<Produto>>(this.apiUrl, {
        params: new HttpParams()
          .set('page', 1)
          .set('pageSize', 1000)
      })
      .pipe(map(response => response.items));
  }

  criar(produto: Omit<Produto, 'id'>): Observable<Produto> {
    return this.http.post<Produto>(this.apiUrl, produto);
  }

  atualizar(id: string, produto: Omit<Produto, 'id'>): Observable<Produto> {
    return this.http.put<Produto>(`${this.apiUrl}/${id}`, produto);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}