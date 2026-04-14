import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { ProdutosComponent } from './pages/produtos/produtos';
import { NotasFiscaisComponent } from './pages/notas-fiscais/notas-fiscais';
import { NotaFiscalDetalheComponent } from './pages/nota-fiscal-detalhe/nota-fiscal-detalhe';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: DashboardComponent },
      { path: 'produtos', component: ProdutosComponent },
      { path: 'notas-fiscais', component: NotasFiscaisComponent },
      { path: 'notas-fiscais/:id', component: NotaFiscalDetalheComponent }
    ]
  }
];