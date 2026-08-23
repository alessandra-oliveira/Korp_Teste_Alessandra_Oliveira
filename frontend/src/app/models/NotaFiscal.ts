import { ItemNotaFiscal } from "./ItemNotaFiscal";

export interface NotaFiscal {
  id: number;
  numero: number;
  status: 'Aberta' | 'Fechada';
  dataCriacao: string;
  itens: ItemNotaFiscal[];
}