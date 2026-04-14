import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { Produto } from '../../core/models/produto.model';
import { NotasFiscaisService } from '../../core/services/notas-fiscais.service';
import { ProdutosService } from '../../core/services/produtos.service';

@Component({
  selector: 'app-notas-fiscais',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './notas-fiscais.html',
  styleUrl: './notas-fiscais.css'
})
export class NotasFiscaisComponent implements OnInit {
  private notasFiscaisService = inject(NotasFiscaisService);
  private produtosService = inject(ProdutosService);
  private toastr = inject(ToastrService);
  private router = inject(Router);

  notas = signal<NotaFiscal[]>([]);
  produtos = signal<Produto[]>([]);
  carregando = signal(false);
  modalAberto = signal(false);

  busca = signal('');
  filtroStatus = signal<'TODAS' | 'ABERTA' | 'FECHADA'>('TODAS');

  novaNota = {
    numero: '',
    dataEmissao: ''
  };

  linhasProdutos = signal<
    {
      codigoProduto: string;
      quantidade: number;
    }[]
  >([]);

  ngOnInit(): void {
    this.carregarNotas();
  }

  carregarNotas() {
    this.carregando.set(true);

    this.notasFiscaisService.listarTodos().subscribe({
      next: (dados) => {
        this.notas.set(dados);
        this.carregando.set(false);
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao carregar notas fiscais', erro);
        this.toastr.error('Não foi possível carregar as notas fiscais.', 'Erro');
        this.carregando.set(false);
      }
    });
  }

  abrirModal() {
    this.modalAberto.set(true);
    this.novaNota = {
      numero: this.gerarProximoNumero(),
      dataEmissao: this.gerarDataHoraLocal()
    };
    this.linhasProdutos.set([]);
    this.carregarProdutos();
  }

  fecharModal() {
    this.modalAberto.set(false);
    this.linhasProdutos.set([]);
  }

  carregarProdutos() {
    this.produtosService.listarTodos().subscribe({
      next: (dados) => {
        this.produtos.set(dados);
      },
      error: () => {
        this.toastr.error('Não foi possível carregar os produtos.', 'Erro');
      }
    });
  }

  gerarProximoNumero(): string {
    const numeros = this.notas()
      .map(n => Number((n.numero || '').replace(/\D/g, '')))
      .filter(n => !isNaN(n));

    const proximo = numeros.length ? Math.max(...numeros) + 1 : 1;
    return String(proximo).padStart(6, '0');
  }

  gerarDataHoraLocal(): string {
    const agora = new Date();
    const ano = agora.getFullYear();
    const mes = String(agora.getMonth() + 1).padStart(2, '0');
    const dia = String(agora.getDate()).padStart(2, '0');
    const hora = String(agora.getHours()).padStart(2, '0');
    const minuto = String(agora.getMinutes()).padStart(2, '0');
    const segundo = String(agora.getSeconds()).padStart(2, '0');

    return `${ano}-${mes}-${dia}T${hora}:${minuto}:${segundo}`;
  }

  formatarDataHora(valor: string): string {
    if (!valor) return '';

    const match = valor.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/);
    if (!match) return valor;

    const [, ano, mes, dia, hora, minuto] = match;
    return `${dia}/${mes}/${ano} ${hora}:${minuto}`;
  }

  atualizarBusca(event: Event) {
    const valor = (event.target as HTMLInputElement).value;
    this.busca.set(valor);
  }

  definirFiltro(status: 'TODAS' | 'ABERTA' | 'FECHADA') {
    this.filtroStatus.set(status);
  }

  abrirDetalhe(nota: NotaFiscal) {
    this.router.navigate(['/notas-fiscais', nota.id]);
  }

  adicionarLinhaProduto() {
    this.linhasProdutos.update(lista => [
      ...lista,
      {
        codigoProduto: '',
        quantidade: 1
      }
    ]);
  }

  removerLinhaProduto(index: number) {
    this.linhasProdutos.update(lista => lista.filter((_, i) => i !== index));
  }

  atualizarCodigoProduto(index: number, valor: string) {
    this.linhasProdutos.update(lista =>
      lista.map((item, i) => i === index ? { ...item, codigoProduto: valor } : item)
    );
  }

  atualizarQuantidade(index: number, valor: number) {
    this.linhasProdutos.update(lista =>
      lista.map((item, i) => i === index ? { ...item, quantidade: valor } : item)
    );
  }

  emitirNota() {
    const linhas = this.linhasProdutos();

    if (linhas.length === 0) {
      this.toastr.warning('Adicione pelo menos um produto na nota.', 'Validação');
      return;
    }

    const itensPayload: {
      codigoProduto: string;
      descricaoProduto: string;
      quantidade: number;
    }[] = [];

    for (const linha of linhas) {
      if (!linha.codigoProduto) {
        this.toastr.warning('Selecione um produto em todas as linhas.', 'Validação');
        return;
      }

      if (linha.quantidade <= 0) {
        this.toastr.warning('A quantidade deve ser maior que zero.', 'Validação');
        return;
      }

      const produto = this.produtos().find(p => p.codigo === linha.codigoProduto);

      if (!produto) {
        this.toastr.warning('Um dos produtos selecionados é inválido.', 'Validação');
        return;
      }

      const quantidadeTotalMesmoProduto = linhas
        .filter(x => x.codigoProduto === linha.codigoProduto)
        .reduce((acc, x) => acc + x.quantidade, 0);

      if (quantidadeTotalMesmoProduto > produto.saldo) {
        this.toastr.warning(
          `A quantidade total do produto ${produto.codigo} é maior que o saldo disponível (${produto.saldo}).`,
          'Validação'
        );
        return;
      }

      itensPayload.push({
        codigoProduto: produto.codigo,
        descricaoProduto: produto.descricao,
        quantidade: linha.quantidade
      });
    }

    this.notasFiscaisService.criar({
      numero: this.novaNota.numero,
      dataEmissao: this.novaNota.dataEmissao,
      itens: itensPayload
    }).subscribe({
      next: () => {
        this.toastr.success('Nota fiscal emitida com sucesso.', 'Sucesso');
        this.fecharModal();
        this.carregarNotas();
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao emitir nota', erro);

        const mensagem =
          erro.error?.error ||
          erro.error?.mensagem ||
          'Não foi possível emitir a nota fiscal.';

        this.toastr.error(mensagem, 'Erro');
      }
    });
  }

  notasFiltradas = computed(() => {
    const termo = this.busca().trim().toLowerCase();
    const filtro = this.filtroStatus();

    return this.notas().filter((nota) => {
      const matchBusca =
        !termo ||
        nota.numero.toLowerCase().includes(termo) ||
        nota.status.toLowerCase().includes(termo);

      const statusNormalizado = nota.status.toUpperCase();

      const matchStatus =
        filtro === 'TODAS' ||
        (filtro === 'ABERTA' && statusNormalizado === 'ABERTA') ||
        (filtro === 'FECHADA' && statusNormalizado === 'FECHADA');

      return matchBusca && matchStatus;
    });
  });

  quantidadeItens(nota: NotaFiscal): number {
    return nota.itens?.reduce((acc, item) => acc + item.quantidade, 0) ?? 0;
  }
}