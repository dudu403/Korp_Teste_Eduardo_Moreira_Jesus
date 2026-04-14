import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { Produto } from '../../core/models/produto.model';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { ProdutosService } from '../../core/services/produtos.service';
import { NotasFiscaisService } from '../../core/services/notas-fiscais.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  private produtosService = inject(ProdutosService);
  private notasFiscaisService = inject(NotasFiscaisService);
  private toastr = inject(ToastrService);
  private router = inject(Router);

  produtos = signal<Produto[]>([]);
  notas = signal<NotaFiscal[]>([]);
  carregandoProdutos = signal(false);
  carregandoNotas = signal(false);

  ngOnInit(): void {
    this.carregarProdutos();
    this.carregarNotas();
  }

  carregarProdutos() {
    this.carregandoProdutos.set(true);

    this.produtosService.listarTodos().subscribe({
      next: (dados) => {
        this.produtos.set(dados);
        this.carregandoProdutos.set(false);
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao carregar produtos do dashboard', erro);
        this.carregandoProdutos.set(false);
        this.toastr.error('Não foi possível carregar os produtos do dashboard.', 'Erro');
      }
    });
  }

  carregarNotas() {
    this.carregandoNotas.set(true);

    this.notasFiscaisService.listarTodos().subscribe({
      next: (dados) => {
        this.notas.set(dados);
        this.carregandoNotas.set(false);
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao carregar notas do dashboard', erro);
        this.carregandoNotas.set(false);
        this.toastr.error('Não foi possível carregar as notas fiscais do dashboard.', 'Erro');
      }
    });
  }

  totalProdutos = computed(() => this.produtos().length);

  totalNotas = computed(() => this.notas().length);

  totalNotasAbertas = computed(() =>
    this.notas().filter(n => n.status === 'Aberta').length
  );

  totalEstoqueBaixo = computed(() =>
    this.produtos().filter(p => p.saldo <= 5).length
  );

  notasRecentes = computed(() =>
    [...this.notas()]
      .sort((a, b) => new Date(b.dataEmissao).getTime() - new Date(a.dataEmissao).getTime())
      .slice(0, 4)
  );

  quantidadeItens(nota: NotaFiscal): number {
    return nota.itens?.reduce((acc, item) => acc + item.quantidade, 0) ?? 0;
  }

  formatarDataHora(valor: string): string {
    if (!valor) return '';

    const match = valor.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/);

    if (!match) return valor;

    const [, ano, mes, dia, hora, minuto] = match;
    return `${dia}/${mes}/${ano} ${hora}:${minuto}`;
  }

  carregandoDashboard(): boolean {
    return this.carregandoProdutos() || this.carregandoNotas();
  }

  irParaProdutos() {
    this.router.navigate(['/produtos']);
  }

  irParaNotasFiscais() {
    this.router.navigate(['/notas-fiscais']);
  }

  irParaNotasAbertas() {
    this.router.navigate(['/notas-fiscais']);
  }

  irParaEstoqueBaixo() {
    this.router.navigate(['/produtos']);
  }

  abrirDetalheNota(nota: NotaFiscal) {
    this.router.navigate(['/notas-fiscais', nota.id]);
  }
}