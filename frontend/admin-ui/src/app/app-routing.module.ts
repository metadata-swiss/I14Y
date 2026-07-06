import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ObUnknownRouteModule} from '@oblique/oblique';
import {HomeComponent} from './home/home.component';
import {UnauthorizedComponent} from './unauthorized/unauthorized.component';
import {CatalogComponent} from './catalog/catalog.component';
import {AuthChildGuard, AuthGuard} from './auth/auth.guard';
import {LoginGuard} from './auth/login.guard';
import {SigninCallbackComponent} from './auth/signin-callback.component';
import {SignoutCallbackComponent} from './auth/signout-callback.component';

const routes: Routes = [
	{path: '', redirectTo: 'home', pathMatch: 'full'},
	{path: 'unauthorized', component: UnauthorizedComponent},
	{path: 'home', component: HomeComponent, canActivate: [AuthGuard]},
	{path: 'signin-callback', component: SigninCallbackComponent},
	{path: 'signout-callback', component: SignoutCallbackComponent},
	{path: 'login', children: [], canActivate: [LoginGuard]},
	{path: 'catalog', redirectTo: 'catalog/all', pathMatch: 'full'},
	{
		path: 'catalog',
		children: [
			{path: 'all', component: CatalogComponent, canActivate: [AuthGuard]},
			{path: 'datasets', component: CatalogComponent, canActivate: [AuthGuard]},
			{path: 'publicservices', component: CatalogComponent, canActivate: [AuthGuard]},
			{path: 'dataservices', component: CatalogComponent, canActivate: [AuthGuard]},
			{path: 'concepts', component: CatalogComponent, canActivate: [AuthGuard]},
			{path: 'mappingtables', component: CatalogComponent, canActivate: [AuthGuard]}
		]
	},
	{
		path: 'catalog/datasets',
		loadChildren: () => import('./datasets/datasets.module').then(m => m.DatasetsModule),
		canActivate: [AuthGuard],
		canActivateChild: [AuthChildGuard]
	},
	{
		path: 'catalog/dataservices',
		loadChildren: () => import('./dataservices/dataservices.module').then(m => m.DataservicesModule),
		canActivate: [AuthGuard],
		canActivateChild: [AuthChildGuard]
	},
	{
		path: 'catalog/publicservices',
		loadChildren: () => import('./publicservices/publicservices.module').then(m => m.PublicServicesModule),
		canActivate: [AuthGuard],
		canActivateChild: [AuthChildGuard]
	},
	{
		path: 'catalog/mappingtables',
		loadChildren: () => import('./mappingtables/mappingtables.module').then(m => m.MappingTablesModule),
		canActivate: [AuthGuard],
		canActivateChild: [AuthChildGuard]
	},
	{path: 'concepts', redirectTo: 'catalog/concepts', pathMatch: 'full'},
	{
		path: 'catalog/concepts',
		loadChildren: () => import('./concepts/concepts.module').then(m => m.ConceptsModule),
		canActivate: [AuthGuard],
		canActivateChild: [AuthChildGuard]
	},
	{path: 'concepts/:conceptId', redirectTo: 'catalog/concepts/:conceptId', pathMatch: 'full'},
	{path: '**', redirectTo: 'unknown-route'}
];

@NgModule({
	imports: [RouterModule.forRoot(routes, {onSameUrlNavigation: 'reload'}), ObUnknownRouteModule],
	exports: [RouterModule]
})
export class AppRoutingModule {}
