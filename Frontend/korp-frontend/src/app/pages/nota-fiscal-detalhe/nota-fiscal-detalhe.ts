import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { NotasFiscaisService } from '../../core/services/notas-fiscais.service';

@Component({
  selector: 'app-nota-fiscal-detalhe',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './nota-fiscal-detalhe.html',
  styleUrl: './nota-fiscal-detalhe.css'
})
export class NotaFiscalDetalheComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private notasFiscaisService = inject(NotasFiscaisService);
  private toastr = inject(ToastrService);

  nota = signal<NotaFiscal | null>(null);
  carregando = signal(false);
  processandoFechamento = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.toastr.error('Nota fiscal não encontrada.', 'Erro');
      this.router.navigate(['/notas-fiscais']);
      return;
    }

    this.carregarNota(id);
  }

  carregarNota(id: string) {
    this.carregando.set(true);

    this.notasFiscaisService.buscarPorId(id).subscribe({
      next: (dados) => {
        this.nota.set(dados);
        this.carregando.set(false);
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao carregar detalhe da nota', erro);
        this.toastr.error('Não foi possível carregar a nota fiscal.', 'Erro');
        this.carregando.set(false);
        this.router.navigate(['/notas-fiscais']);
      }
    });
  }

  totalItens(): number {
    return this.nota()?.itens?.reduce((acc, item) => acc + item.quantidade, 0) ?? 0;
  }

  formatarDataCompleta(valor: string): string {
    if (!valor) return '';

    const match = valor.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/);

    if (!match) return valor;

    const [, ano, mes, dia, hora, minuto] = match;
    return `Emitida em ${dia}/${mes}/${ano} às ${hora}:${minuto}`;
  }

  fecharNota() {
    const notaAtual = this.nota();

    if (!notaAtual) return;
    if (notaAtual.status === 'Fechada') return;

    this.processandoFechamento.set(true);

    this.notasFiscaisService.fechar(notaAtual.id).subscribe({
      next: (notaAtualizada) => {
        this.nota.set(notaAtualizada);
        this.processandoFechamento.set(false);
        this.toastr.success(`NF-e #${notaAtual.numero} fechada com sucesso.`, 'Sucesso');
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao fechar nota fiscal', erro);

        const mensagem =
          erro.error?.error ||
          erro.error?.mensagem ||
          'Não foi possível fechar a nota fiscal.';

        this.processandoFechamento.set(false);
        this.toastr.error(mensagem, 'Erro');
      }
    });
  }

  podeFechar(): boolean {
    return !!this.nota() && this.nota()!.status !== 'Fechada' && !this.processandoFechamento();
  }
}