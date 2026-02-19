import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TerrainProperties {
  id: string;
  titre: string;
  description: string;
  quartier: string;
  commune: string;
  surface: number;
  prix: number;
  prixParM2: number;
  statut: string;
  typeTerrain: string;
  contactNom: string;
  contactTelephone: string;
}

export interface TerrainFeature {
  type: 'Feature';
  properties: TerrainProperties;
  geometry: {
    type: 'Polygon';
    coordinates: number[][][];
  };
}

export interface TerrainGeoJson {
  type: 'FeatureCollection';
  features: TerrainFeature[];
}

@Injectable({
  providedIn: 'root'
})
export class TerrainService {
  private apiUrl = 'http://localhost:5000/api/terrains';

  constructor(private http: HttpClient) {}

  getAll(): Observable<TerrainGeoJson> {
    return this.http.get<TerrainGeoJson>(`${this.apiUrl}/geojson`);
  }

  search(query: string, commune?: string, quartier?: string): Observable<TerrainGeoJson> {
    let params = `q=${encodeURIComponent(query)}`;
    if (commune) params += `&commune=${encodeURIComponent(commune)}`;
    if (quartier) params += `&quartier=${encodeURIComponent(quartier)}`;
    return this.http.get<TerrainGeoJson>(`${this.apiUrl}/geojson/search?${params}`);
  }

  getById(id: string): Observable<TerrainProperties> {
    return this.http.get<TerrainProperties>(`${this.apiUrl}/${id}`);
  }

  create(terrain: CreateTerrainRequest): Observable<TerrainProperties> {
    return this.http.post<TerrainProperties>(this.apiUrl, terrain);
  }

  getSuggestions(query: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/suggestions?q=${encodeURIComponent(query)}`);
  }
}

export interface CreateTerrainRequest {
  titre: string;
  description?: string;
  quartier: string;
  commune: string;
  surface: number;
  prix: number;
  prixParM2?: number;
  statut?: string;
  typeTerrain?: string;
  coordinates: number[][];
  contactNom?: string;
  contactTelephone?: string;
}
