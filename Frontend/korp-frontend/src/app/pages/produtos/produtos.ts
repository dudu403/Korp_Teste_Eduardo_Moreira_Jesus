import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { Produto } from '../../core/models/produto.model';
import { ProdutosService } from '../../core/services/produtos.service';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './produtos.html',
  styleUrl: './produtos.css'
})
export class ProdutosComponent implements OnInit {
  private produtosService = inject(ProdutosService);
  private toastr = inject(ToastrService);

  modalAberto = signal(false);
  modalExcluirAberto = signal(false);
  busca = signal('');
  produtos = signal<Produto[]>([]);
  carregando = signal(false);
  editandoId = signal<string | null>(null);
  produtoParaExcluir = signal<Produto | null>(null);

  novoProduto = {
    codigo: '',
    descricao: '',
    saldo: 0
  };

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos() {
    this.carregando.set(true);

    this.produtosService.listarTodos().subscribe({
      next: (dados) => {
        this.produtos.set(dados);
        this.carregando.set(false);
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao carregar produtos', erro);
        this.toastr.error('Não foi possível carregar os produtos.', 'Erro');
        this.carregando.set(false);
      }
    });
  }

  abrirModal() {
    this.editandoId.set(null);
    this.modalAberto.set(true);
    this.novoProduto = {
      codigo: '',
      descricao: '',
      saldo: 0
    };
  }

  abrirModalEdicao(produto: Produto) {
    this.editandoId.set(produto.id);
    this.modalAberto.set(true);
    this.novoProduto = {
      codigo: produto.codigo,
      descricao: produto.descricao,
      saldo: produto.saldo
    };
  }

  fecharModal() {
    this.modalAberto.set(false);
    this.editandoId.set(null);
    this.novoProduto = {
      codigo: '',
      descricao: '',
      saldo: 0
    };
  }

  abrirModalExcluir(produto: Produto) {
    this.produtoParaExcluir.set(produto);
    this.modalExcluirAberto.set(true);
  }

  fecharModalExcluir() {
    this.modalExcluirAberto.set(false);
    this.produtoParaExcluir.set(null);
  }

  atualizarBusca(event: Event) {
    const valor = (event.target as HTMLInputElement).value;
    this.busca.set(valor);
  }

  get produtosFiltrados() {
    const termo = this.busca().trim().toLowerCase();

    if (!termo) return this.produtos();

    return this.produtos().filter((produto) =>
      produto.codigo.toLowerCase().includes(termo) ||
      produto.descricao.toLowerCase().includes(termo)
    );
  }

  salvarProduto() {
    const codigo = this.novoProduto.codigo.trim();
    const descricao = this.novoProduto.descricao.trim();

    if (!codigo) {
      this.toastr.warning('O código do produto é obrigatório.', 'Validação');
      return;
    }

    if (codigo.length > 20) {
      this.toastr.warning('O código deve ter no máximo 20 caracteres.', 'Validação');
      return;
    }

    if (!descricao) {
      this.toastr.warning('A descrição do produto é obrigatória.', 'Validação');
      return;
    }

    if (descricao.length > 100) {
      this.toastr.warning('A descrição deve ter no máximo 100 caracteres.', 'Validação');
      return;
    }

    if (this.novoProduto.saldo < 0) {
      this.toastr.warning('O saldo não pode ser negativo.', 'Validação');
      return;
    }

    const payload = {
      codigo,
      descricao,
      saldo: this.novoProduto.saldo
    };

    const id = this.editandoId();

    if (id) {
      this.produtosService.atualizar(id, payload).subscribe({
        next: () => {
          this.toastr.success('Produto atualizado com sucesso.', 'Sucesso');
          this.fecharModal();
          this.carregarProdutos();
        },
        error: (erro: HttpErrorResponse) => {
          console.error('Erro ao atualizar produto', erro);

          const mensagem =
            erro.error?.error ||
            erro.error?.mensagem ||
            'Não foi possível atualizar o produto.';

          this.toastr.error(mensagem, 'Erro');
        }
      });

      return;
    }

    this.produtosService.criar(payload).subscribe({
      next: () => {
        this.toastr.success('Produto cadastrado com sucesso.', 'Sucesso');
        this.fecharModal();
        this.carregarProdutos();
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao cadastrar produto', erro);

        const mensagem =
          erro.error?.error ||
          erro.error?.mensagem ||
          'Não foi possível cadastrar o produto.';

        this.toastr.error(mensagem, 'Erro');
      }
    });
  }

  confirmarExclusao() {
    const produto = this.produtoParaExcluir();

    if (!produto) return;

    this.produtosService.excluir(produto.id).subscribe({
      next: () => {
        this.toastr.success('Produto excluído com sucesso.', 'Sucesso');
        this.fecharModalExcluir();
        this.carregarProdutos();
      },
      error: (erro: HttpErrorResponse) => {
        console.error('Erro ao excluir produto', erro);

        const mensagemBackend =
          erro.error?.error ||
          erro.error?.mensagem ||
          '';

        const mensagem =
          mensagemBackend ||
          'Não foi possível excluir o produto.';

        this.toastr.error(mensagem, 'Erro');
      }
    });
  }
}