import {inject, NgModule} from '@angular/core';
import {Router, RouterModule, Routes} from '@angular/router';
import {ObUnknownRouteComponent, ObUnknownRouteModule} from '@oblique/oblique';
import {LanguageGuard} from './shared/guards/language.guard';

const routes: Routes = [
	{path: '', redirectTo: 'de/home', pathMatch: 'full'},
	{path: 'home', redirectTo: 'de/home', pathMatch: 'full'},
	{
		path: ':langId',
		canActivate: [LanguageGuard],
		children: [
			{
				path: '',
				redirectTo: 'home',
				pathMatch: 'full'
			},
			{
				path: 'catalog',
				redirectTo: 'catalog/all',
				pathMatch: 'full'
			},
			{
				path: 'dataservices',
				redirectTo: 'catalog/dataservices',
				pathMatch: 'full'
			},
			{
				path: 'datasets',
				redirectTo: 'catalog/datasets',
				pathMatch: 'full'
			},
			{
				path: 'publicservices',
				redirectTo: 'catalog/publicservices',
				pathMatch: 'full'
			},
			{
				path: 'metasearch/geocat',
				redirectTo: 'catalog/metasearch/geocat',
				pathMatch: 'full'
			},
			{
				path: 'concepts',
				redirectTo: 'catalog/concepts',
				pathMatch: 'full'
			},
			{
				path: 'dataservices/:dataServiceId',
				redirectTo: 'catalog/dataservices/:dataServiceId'
			},
			{
				path: 'publicservices/:publicServiceId',
				redirectTo: 'catalog/publicServices/:publicServiceId'
			},
			{
				path: 'datasets/:configIdentifier',
				redirectTo: 'catalog/datasets/:configIdentifier'
			},
			{
				path: 'catalog/datasets/:configIdentifier/distributions',
				redirectTo: activatedRouteSnapshot => {
					return inject(Router).createUrlTree(
						[activatedRouteSnapshot.params.langId, 'catalog', 'datasets', activatedRouteSnapshot.params.configIdentifier, 'description'],
						{fragment: 'section-distributions'}
					);
				},
				pathMatch: 'full'
			},
			{
				path: 'concepts/:conceptId',
				redirectTo: 'catalog/concepts/:conceptId'
			},
			{
				path: 'home',
				loadChildren: () => import('./home/home.module').then(m => m.HomeModule)
			},
			{
				path: 'news',
				loadChildren: () => import('./news/news.module').then(m => m.NewsModule)
			},
			{
				path: 'organisations',
				loadChildren: () => import('./organisations/organisations.module').then(m => m.OrganisationsModule)
			},
			{
				path: 'catalog/all',
				loadChildren: () => import('./catalog/catalog.module').then(m => m.CatalogModule)
			},
			{
				path: 'catalog/datasets',
				loadChildren: () => import('./catalog/catalog.module').then(m => m.CatalogModule)
			},
			{
				path: 'catalog/datasets/:configIdentifier',
				loadChildren: () => import('./datasets/datasets.module').then(m => m.DatasetsModule)
			},
			{
				path: 'catalog/dataservices',
				loadChildren: () => import('./catalog/catalog.module').then(m => m.CatalogModule)
			},
			{
				path: 'catalog/dataservices/:dataServiceId',
				loadChildren: () => import('./data-services/data-services.module').then(m => m.DataServicesModule)
			},
			{
				path: 'catalog/publicservices',
				loadChildren: () => import('./catalog/catalog.module').then(m => m.CatalogModule)
			},
			{
				path: 'catalog/publicservices/:publicServiceId',
				loadChildren: () => import('./public-services/public-service.module').then(m => m.PublicServiceModule)
			},
			{
				path: 'catalog/mappingtables',
				loadChildren: () => import('./catalog/catalog.module').then(m => m.CatalogModule)
			},
			{
				path: 'catalog/mappingtables/:mappingTableId',
				loadChildren: () => import('./mappingtables/mappingtables.module').then(m => m.MappingTablesModule)
			},
			{
				path: 'catalog/metasearch/geocat',
				loadChildren: () => import('./metasearch/geocat/geocat.module').then(m => m.GeocatModule)
			},
			{
				path: 'catalog/metasearch/opendata',
				loadChildren: () => import('./metasearch/opendata/opendata.module').then(m => m.OpendataModule)
			},
			{
				path: 'catalog/concepts',
				loadChildren: () => import('./catalog/catalog.module').then(m => m.CatalogModule)
			},
			{
				path: 'catalog/concepts/:conceptId',
				loadChildren: () => import('./concepts/concepts.module').then(m => m.ConceptsModule)
			}
		]
	},
	{path: '**', component: ObUnknownRouteComponent}
];

@NgModule({
	imports: [
		RouterModule.forRoot(routes, {
			enableTracing: false,
			anchorScrolling: 'enabled',
			onSameUrlNavigation: 'reload',
			scrollPositionRestoration: 'enabled',
			scrollOffset: () => [0, 108]
		}),
		ObUnknownRouteModule
	],
	exports: [RouterModule]
})
export class AppRoutingModule {}
