import { Routes } from '@angular/router';

import { ListaProdutosComponent } from './components/produtos/produto-list/produto-list';
import { FormularioProdutoComponent } from './components/produtos/produto-form/produto-form';
import { ListaNotasFiscaisComponent } from './components/nota-fiscal/notas-fiscais/notas-fiscais';
import { NovaNotaFiscalComponent } from './components/nota-fiscal/nova-nota-fiscal/nova-nota-fiscal';
import { DetalheNotaFiscalComponent } from './components/nota-fiscal/detalhe-nota-fiscal/detalhe-nota-fiscal';

export const routes: Routes = [
  { path: 'produtos', component: ListaProdutosComponent },

  { path: 'produtos/novo', component: FormularioProdutoComponent },

  { path: 'produtos/:id/editar', component: FormularioProdutoComponent },

  { path: 'notas-fiscais', component: ListaNotasFiscaisComponent },

  { path: 'notas-fiscais/nova', component: NovaNotaFiscalComponent },

  { path: 'notas-fiscais/:id', component: DetalheNotaFiscalComponent },

  { path: '', redirectTo: '/produtos', pathMatch: 'full' },
];
