import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as L from 'leaflet';
import 'leaflet-draw';
import { TerrainService, TerrainFeature, TerrainGeoJson, CreateTerrainRequest } from '../../services/terrain.service';
import { SearchService } from '../../services/search.service';
import { Subscription } from 'rxjs';
import { toast } from 'ngx-sonner';
import { HlmSpinner } from '@spartan-ng/helm/spinner';

const iconRetinaUrl = 'assets/marker-icon-2x.png';
const iconUrl = 'assets/marker-icon.png';
const shadowUrl = 'assets/marker-shadow.png';
const iconDefault = L.icon({
  iconRetinaUrl,
  iconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  tooltipAnchor: [16, -28],
  shadowSize: [41, 41]
});
L.Marker.prototype.options.icon = iconDefault;

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, FormsModule, HlmSpinner],
  templateUrl: './map.component.html',
  styles: [`
    :host {
      display: block;
      width: 100%;
      height: 100%;
      position: relative;
    }
    #map {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      z-index: 1;
    }
  `]
})
export class MapComponent implements OnInit, OnDestroy {
  private terrainService = inject(TerrainService);
  private searchService = inject(SearchService);
  private map!: L.Map;
  private geojsonLayer?: L.GeoJSON;
  private subscription?: Subscription;
  private searchSubscription?: Subscription;
  private drawnItems: L.FeatureGroup = new L.FeatureGroup();
  
  selectedTerrain: TerrainFeature | null = null;
  showDrawForm = false;
  newTerrain: CreateTerrainRequest = this.getEmptyTerrain();
  private currentPolygon?: L.Polygon;
  isSaving = false;

  ngOnInit(): void {
    setTimeout(() => {
      this.initMap();
      this.loadTerrains();
    }, 0);
    
    this.searchSubscription = this.searchService.search$.subscribe(query => {
      this.searchTerrains(query);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.searchSubscription?.unsubscribe();
    if (this.map) {
      this.map.remove();
    }
  }

  private initMap(): void {
    // Centre sur Conakry
    const conakryCenter: L.LatLngExpression = [9.5092, -13.7122];
    
    const conakryBounds = L.latLngBounds(
      L.latLng(9.65, -13.80),  
      L.latLng(9.00, -13.60)  
    );

    this.map = L.map('map', {
      center: conakryCenter,
      zoom: 14,
      minZoom: 10,  
      maxZoom: 20,
      maxBounds: conakryBounds,
      maxBoundsViscosity: 0.8  
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19,
      crossOrigin: true  
    }).addTo(this.map);

    this.map.on('load', () => {
      console.log('Map loaded successfully');
    });

    this.map.on('tileerror', (error) => {
      console.error('Tile loading error:', error);
    });
    
    setTimeout(() => {
      this.map.invalidateSize();
    }, 100);

    this.initDrawControls();
  }

  private initDrawControls(): void {
    this.map.addLayer(this.drawnItems);

    const drawControl = new L.Control.Draw({
      position: 'topright',
      draw: {
        polygon: {
          allowIntersection: false,
          showArea: true,
          drawError: {
            color: '#e74c3c',
            timeout: 1000
          },
          shapeOptions: {
            stroke: true,
            color: '#3b82f6',
            weight: 2,
            opacity: 0.5,
            fill: true,
            fillColor: '#3b82f6',
            fillOpacity: 0.2,
            clickable: true
          },
          icon: new L.DivIcon({
            iconSize: new L.Point(8, 8),
            className: 'leaflet-div-icon leaflet-editing-icon'
          }),
          touchIcon: new L.DivIcon({
            iconSize: new L.Point(20, 20),
            className: 'leaflet-div-icon leaflet-editing-icon leaflet-touch-icon'
          }),
          guidelineDistance: 20,
          maxGuideLineLength: 4000,
          metric: true,
          feet: false,
          nautic: false,
          precision: {}
        },
        polyline: false,
        circle: false,
        rectangle: false,
        marker: false,
        circlemarker: false
      },
      edit: {
        featureGroup: this.drawnItems,
        remove: true
      }
    });

    this.map.addControl(drawControl);

    this.map.on(L.Draw.Event.CREATED, (event: any) => {
      const layer = event.layer;
      this.currentPolygon = layer;
      this.drawnItems.addLayer(layer);
      
      const coordinates = (layer as L.Polygon).getLatLngs()[0] as L.LatLng[];
      const coords = coordinates.map(c => [c.lng, c.lat]);
      coords.push([coords[0][0], coords[0][1]]);
      
      this.newTerrain.coordinates = coords;
      this.calculateArea(coordinates);
      this.showDrawForm = true;
    });
  }

  private calculateArea(coordinates: L.LatLng[]): void {
    let area = 0;
    const earthRadius = 6371000;
    
    for (let i = 0; i < coordinates.length; i++) {
      const j = (i + 1) % coordinates.length;
      const lat1 = coordinates[i].lat * Math.PI / 180;
      const lat2 = coordinates[j].lat * Math.PI / 180;
      const lng1 = coordinates[i].lng * Math.PI / 180;
      const lng2 = coordinates[j].lng * Math.PI / 180;
      
      area += (lng2 - lng1) * (2 + Math.sin(lat1) + Math.sin(lat2));
    }
    
    area = Math.abs(area * earthRadius * earthRadius / 2);
    this.newTerrain.surface = Math.round(area * 100) / 100;
    
    if (this.newTerrain.prix && this.newTerrain.surface > 0) {
      this.newTerrain.prixParM2 = Math.round((this.newTerrain.prix / this.newTerrain.surface) * 100) / 100;
    }
  }

  private getEmptyTerrain(): CreateTerrainRequest {
    return {
      titre: '',
      description: '',
      quartier: '',
      commune: '',
      surface: 0,
      prix: 0,
      prixParM2: 0,
      statut: 'Disponible',
      typeTerrain: 'Résidentiel',
      coordinates: [],
      contactNom: '',
      contactTelephone: ''
    };
  }

  onPrixChange(): void {
    if (this.newTerrain.prix && this.newTerrain.surface > 0) {
      this.newTerrain.prixParM2 = Math.round((this.newTerrain.prix / this.newTerrain.surface) * 100) / 100;
    }
  }

  saveTerrain(): void {
    if (!this.newTerrain.titre || !this.newTerrain.quartier || !this.newTerrain.commune) {
      toast.error('Veuillez remplir tous les champs obligatoires');
      return;
    }

    this.isSaving = true;
    this.terrainService.create(this.newTerrain).subscribe({
      next: () => {
        toast.success('Terrain enregistré avec succès!', {
          description: `${this.newTerrain.titre} a été ajouté à la carte`
        });
        this.isSaving = false;
        this.cancelDraw();
        this.loadTerrains();
      },
      error: (err) => {
        console.error('Erreur lors de la sauvegarde:', err);
        toast.error('Erreur lors de la sauvegarde du terrain', {
          description: 'Veuillez réessayer ou contacter le support'
        });
        this.isSaving = false;
      }
    });
  }

  cancelDraw(): void {
    this.showDrawForm = false;
    if (this.currentPolygon) {
      this.drawnItems.removeLayer(this.currentPolygon);
      this.currentPolygon = undefined;
    }
    this.newTerrain = this.getEmptyTerrain();
  }

  loadTerrains(): void {
    this.terrainService.getAll().subscribe({
      next: (data) => this.displayTerrains(data),
      error: (err) => console.error('Error loading terrains:', err)
    });
  }

  private displayTerrains(data: TerrainGeoJson): void {
    if (this.geojsonLayer) {
      this.map.removeLayer(this.geojsonLayer);
    }

    this.geojsonLayer = L.geoJSON(data as any, {
      style: (feature) => ({
        color: feature?.properties?.statut === 'Disponible' ? '#22c55e' : '#eab308',
        weight: 2,
        fillOpacity: 0.3,
        fillColor: feature?.properties?.statut === 'Disponible' ? '#22c55e' : '#eab308'
      }),
      onEachFeature: (feature, layer) => {
        layer.on('click', () => {
          this.selectedTerrain = feature as TerrainFeature;
        });
        
        layer.bindTooltip(feature.properties.titre, {
          permanent: false,
          direction: 'top'
        });
      }
    }).addTo(this.map);

    if (data.features.length > 0) {
      const bounds = this.geojsonLayer.getBounds();
      this.map.fitBounds(bounds, { 
        padding: [50, 50],
        maxZoom: 15  
      });
    }
  }

  closeSidebar(): void {
    this.selectedTerrain = null;
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('fr-GN').format(price);
  }

  searchTerrains(query: string): void {
    if (!query || query.trim().length === 0) {
      this.loadTerrains();
      return;
    }

    this.terrainService.search(query).subscribe({
      next: (data) => {
        this.displayTerrains(data);
        
        if (data.features.length > 0) {
          this.zoomToSearchResults(data);
          toast.success(`${data.features.length} terrain(s) trouvé(s)`);
        } else {
          toast.info('Aucun terrain trouvé pour cette recherche');
        }
      },
      error: (err) => {
        console.error('Error searching terrains:', err);
        toast.error('Erreur lors de la recherche');
      }
    });
  }

  private zoomToSearchResults(data: TerrainGeoJson): void {
    if (!this.geojsonLayer || data.features.length === 0) return;

    const bounds = this.geojsonLayer.getBounds();
    
    if (data.features.length === 1) {
      const feature = data.features[0];
      const center = bounds.getCenter();
      this.map.setView(center, 17, {
        animate: true,
        duration: 1
      });
      
      setTimeout(() => {
        this.selectedTerrain = feature;
      }, 500);
    } else {
      this.map.fitBounds(bounds, {
        padding: [80, 80],
        maxZoom: 16,
        animate: true,
        duration: 1
      });
    }
  }
}
