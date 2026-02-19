import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private darkClass = 'dark';
  
  constructor() {
    this.loadTheme();
  }

  toggleTheme() {
    const html = document.documentElement;
    html.classList.toggle(this.darkClass);

    const isDark = html.classList.contains(this.darkClass);
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
  }

  loadTheme() {
    const theme = localStorage.getItem('theme');
    if (theme === 'dark') {
      document.documentElement.classList.add(this.darkClass);
    }
  }

  isDark(): boolean {
    return document.documentElement.classList.contains(this.darkClass);
  }
  
}
