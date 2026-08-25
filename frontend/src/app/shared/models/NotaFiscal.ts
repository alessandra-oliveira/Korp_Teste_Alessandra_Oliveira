import { ItemNotaFiscal } from "./ItemNotaFiscal";

export interface NotaFiscal {
  id: number;
  numero: number;
  status: 'Aberta' | 'Fechada' | 'Processada' | 'Processando';
  dataCriacao: string;
  itens: ItemNotaFiscal[];
}