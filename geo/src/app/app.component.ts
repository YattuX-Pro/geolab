import { Component, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeToggle } from '../component/theme-toggle/theme-toggle';
import { TopNav } from '../component/top-nav/top-nav';
import { MapComponent } from './pages/map/map.component';
import { HlmToaster } from '@spartan-ng/helm/sonner';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ThemeToggle, TopNav, MapComponent, HlmToaster],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'geo';
  @ViewChild(MapComponent) mapComponent?: MapComponent;

  onSearch(query: string): void {
    this.mapComponent?.searchTerrains(query);
  }
}
