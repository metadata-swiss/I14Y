import {SharedModule} from 'src/app/shared/shared.module';
import {NgModule} from '@angular/core';
import {HomeRoutingModule} from './home-routing.module';
import {TranslateModule} from '@ngx-translate/core';
import {HomeComponent} from './home.component';

@NgModule({
	declarations: [HomeComponent],
	imports: [HomeRoutingModule, SharedModule, TranslateModule],
	exports: [HomeComponent]
})
export class HomeModule {}
