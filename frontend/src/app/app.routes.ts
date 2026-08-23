import { Routes } from '@angular/router';

import { ProdutoListComponent } from './produtos/produto-list/produto-list';
import { ProdutoFormComponent } from './produtos/produto-form/produto-form';
import { NotasFiscaisComponent } from './nota-fiscal/notas-fiscais/notas-fiscais';
import { NovaNotaFiscal } from './nota-fiscal/nova-nota-fiscal/nova-nota-fiscal';
import { DetalheNotaFiscal} from './nota-fiscal/detalhe-nota-fiscal/detalhe-nota-fiscal';

export const routes: Routes = [

  { path: 'produtos', component: ProdutoListComponent },

  { path: 'produtos/novo', component: ProdutoFormComponent },

  { path: 'produtos/:id/editar', component: ProdutoFormComponent },

  { path: 'notas-fiscais', component: NotasFiscaisComponent },

  { path: 'notas-fiscais/nova', component: NovaNotaFiscal },

  { path: 'notas-fiscais/:id', component: DetalheNotaFiscal },

  { path: '', redirectTo: '/produtos', pathMatch: 'full' }

];