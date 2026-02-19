import { Component, EventEmitter, Output, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmSpinner } from '@spartan-ng/helm/spinner';
import { TerrainService } from '../../app/services/terrain.service';
import { SearchService } from '../../app/services/search.service';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';


@Component({
  selector: 'app-top-nav',
  imports: [CommonModule, FormsModule, RouterModule, HlmButtonImports, HlmSpinner],
  templateUrl: './top-nav.html',
  styles: ``,
})
export class TopNav implements OnInit, OnDestroy {
  private terrainService = inject(TerrainService);
  private searchService = inject(SearchService);
  
  searchQuery = '';
  suggestions: string[] = [];
  showSuggestions = false;
  isSearching = false;
  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;
  
  @Output() search = new EventEmitter<string>();

  ngOnInit(): void {
    this.initSearchSubscription();
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  private initSearchSubscription(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => {
        if (query.length < 2) {
          this.suggestions = [];
          return [];
        }
        this.isSearching = true;
        return this.terrainService.getSuggestions(query);
      })
    ).subscribe({
      next: (suggestions) => {
        this.suggestions = suggestions;
        this.isSearching = false;
      },
      error: (err) => {
        console.error('Error fetching suggestions:', err);
        this.isSearching = false;
        this.suggestions = [];
      }
    });
  }

  onSearchInput(): void {
    this.searchSubject.next(this.searchQuery);
  }

  onSearchBlur(): void {
    setTimeout(() => {
      this.showSuggestions = false;
    }, 200);
  }

  selectSuggestion(suggestion: string): void {
    this.searchQuery = suggestion;
    this.showSuggestions = false;
    this.onSearch();
  }

  onSearch(): void {
    this.search.emit(this.searchQuery);
    this.searchService.emitSearch(this.searchQuery);
    this.showSuggestions = false;
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.suggestions = [];
    this.showSuggestions = false;
    this.search.emit('');
    this.searchService.emitSearch('');
  }
}
