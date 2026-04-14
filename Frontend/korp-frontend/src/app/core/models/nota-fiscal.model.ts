import { ItemNotaFiscal } from './item-nota-fiscal.model';

export interface NotaFiscal {
  id: string;
  numero: string;
  dataEmissao: string;
  status: string;
  itens: ItemNotaFiscal[];
}