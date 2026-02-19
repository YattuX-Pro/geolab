import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import OSM from 'ol/source/OSM';
import TileWMS from 'ol/source/TileWMS';
import { fromLonLat, transformExtent } from 'ol/proj';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmSpinner } from '@spartan-ng/helm/spinner';

interface WmsLayer {
  name: string;
  title: string;
  visible: boolean;
  bbox?: [number, number, number, number]; // [minx, miny, maxx, maxy] en EPSG:4326
  layer?: TileLayer<TileWMS>;
}

@Component({
  selector: 'app-topography',
  standalone: true,
  imports: [CommonModule, FormsModule, HlmButtonImports, HlmSpinner],
  templateUrl: './topography.component.html',
  styles: [`
    :host {
      display: block;
      height: 100%;
      overflow: hidden;
    }
  `]
})
export class TopographyComponent implements OnInit, AfterViewInit, OnDestroy {
  private map!: Map;
  
  // Configuration GeoServer
  geoserverUrl = 'http://localhost:8085/geoserver';
  workspace = 'test_geoserver';
  
  isLoading = true;
  availableLayers: WmsLayer[] = [];
  errorMessage = '';
  showLayerPanel = true;
  globalBbox: [number, number, number, number] | null = null;
  selectedLayer: WmsLayer | null = null;

  ngOnInit(): void {
    this.loadAvailableLayers();
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.initMap(), 0);
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.setTarget(undefined);
    }
  }

  private initMap(): void {
    this.map = new Map({
      target: 'topography-map',
      layers: [
        new TileLayer({
          source: new OSM()
        })
      ],
      view: new View({
        center: fromLonLat([0, 0]),
        zoom: 2
      })
    });

    this.map.on('loadstart', () => {
      this.isLoading = true;
    });

    this.map.on('loadend', () => {
      this.isLoading = false;
    });

    setTimeout(() => {
      this.map.updateSize();
      this.isLoading = false;
      // Zoomer sur les données si disponibles
      if (this.globalBbox) {
        this.zoomToDataExtent();
      }
    }, 100);
  }

  private loadAvailableLayers(): void {
    // Récupérer les couches disponibles via GetCapabilities
    const capabilitiesUrl = `${this.geoserverUrl}/${this.workspace}/wms?service=WMS&version=1.1.1&request=GetCapabilities`;
    
    fetch(capabilitiesUrl)
      .then(response => response.text())
      .then(xml => {
        const parser = new DOMParser();
        const doc = parser.parseFromString(xml, 'text/xml');
        const layers = doc.querySelectorAll('Layer > Layer');
        
        this.availableLayers = [];
        let globalMinX = Infinity, globalMinY = Infinity;
        let globalMaxX = -Infinity, globalMaxY = -Infinity;
        
        layers.forEach((layer) => {
          const name = layer.querySelector('Name')?.textContent || '';
          const title = layer.querySelector('Title')?.textContent || name;
          
          // Récupérer la bounding box de la couche (support WMS 1.1.1 et 1.3.0)
          let bbox: [number, number, number, number] | undefined;
          
          // Essayer LatLonBoundingBox (WMS 1.1.1)
          const latLonBbox = layer.querySelector('LatLonBoundingBox');
          if (latLonBbox) {
            const minx = parseFloat(latLonBbox.getAttribute('minx') || '0');
            const miny = parseFloat(latLonBbox.getAttribute('miny') || '0');
            const maxx = parseFloat(latLonBbox.getAttribute('maxx') || '0');
            const maxy = parseFloat(latLonBbox.getAttribute('maxy') || '0');
            bbox = [minx, miny, maxx, maxy];
          }
          
          // Essayer EX_GeographicBoundingBox (WMS 1.3.0)
          if (!bbox) {
            const exBbox = layer.querySelector('EX_GeographicBoundingBox');
            if (exBbox) {
              const westBound = exBbox.querySelector('westBoundLongitude')?.textContent;
              const eastBound = exBbox.querySelector('eastBoundLongitude')?.textContent;
              const southBound = exBbox.querySelector('southBoundLatitude')?.textContent;
              const northBound = exBbox.querySelector('northBoundLatitude')?.textContent;
              if (westBound && eastBound && southBound && northBound) {
                bbox = [
                  parseFloat(westBound),
                  parseFloat(southBound),
                  parseFloat(eastBound),
                  parseFloat(northBound)
                ];
              }
            }
          }
          
          // Essayer BoundingBox avec CRS:84 ou EPSG:4326
          if (!bbox) {
            const boundingBoxes = layer.querySelectorAll('BoundingBox');
            boundingBoxes.forEach((bb) => {
              const crs = bb.getAttribute('CRS') || bb.getAttribute('SRS');
              if (crs === 'CRS:84' || crs === 'EPSG:4326') {
                const minx = parseFloat(bb.getAttribute('minx') || '0');
                const miny = parseFloat(bb.getAttribute('miny') || '0');
                const maxx = parseFloat(bb.getAttribute('maxx') || '0');
                const maxy = parseFloat(bb.getAttribute('maxy') || '0');
                bbox = [minx, miny, maxx, maxy];
              }
            });
          }
          
          if (bbox) {
            // Mettre à jour la bbox globale
            globalMinX = Math.min(globalMinX, bbox[0]);
            globalMinY = Math.min(globalMinY, bbox[1]);
            globalMaxX = Math.max(globalMaxX, bbox[2]);
            globalMaxY = Math.max(globalMaxY, bbox[3]);
          }
          
          if (name) {
            this.availableLayers.push({
              name: name,
              title: title,
              visible: false,
              bbox: bbox
            });
          }
        });
        
        // Stocker la bbox globale
        if (globalMinX !== Infinity) {
          this.globalBbox = [globalMinX, globalMinY, globalMaxX, globalMaxY];
          // Zoomer sur l'étendue des données
          if (this.map) {
            this.zoomToDataExtent();
          }
        }

        // Si des couches sont trouvées, activer toutes par défaut
        if (this.availableLayers.length > 0) {
          this.availableLayers.forEach(layer => this.toggleLayer(layer));
        }
      })
      .catch(err => {
        console.error('Erreur lors du chargement des couches:', err);
        this.errorMessage = 'Impossible de charger les couches GeoServer. Vérifiez que GeoServer est démarré sur le port 8085.';
        
        // Ajouter des couches par défaut
        this.addDefaultLayers();
      });
  }

  private addDefaultLayers(): void {
    // Couches par défaut si GetCapabilities échoue - utiliser le workspace configuré
    this.availableLayers = [
      { name: `${this.workspace}:altitude`, title: 'Altitude', visible: false }
    ];
  }

  private zoomToDataExtent(): void {
    if (!this.globalBbox || !this.map) return;
    
    const extent = transformExtent(
      this.globalBbox,
      'EPSG:4326',
      'EPSG:3857'
    );
    this.map.getView().fit(extent, { padding: [50, 50, 50, 50], duration: 500 });
  }

  toggleLayer(wmsLayer: WmsLayer): void {
    wmsLayer.visible = !wmsLayer.visible;

    if (wmsLayer.visible) {
      // Créer et ajouter la couche WMS
      const tileLayer = new TileLayer({
        source: new TileWMS({
          url: `${this.geoserverUrl}/${this.workspace}/wms`,
          params: {
            'LAYERS': wmsLayer.name,
            'TILED': true,
            'FORMAT': 'image/png',
            'TRANSPARENT': true
          },
          serverType: 'geoserver',
          crossOrigin: 'anonymous'
        }),
        opacity: 0.8
      });

      wmsLayer.layer = tileLayer;
      this.map.addLayer(tileLayer);
    } else {
      // Retirer la couche
      if (wmsLayer.layer) {
        this.map.removeLayer(wmsLayer.layer);
        wmsLayer.layer = undefined;
      }
    }
  }

  toggleAllLayers(visible: boolean): void {
    this.availableLayers.forEach(layer => {
      if (layer.visible !== visible) {
        this.toggleLayer(layer);
      }
    });
  }

  zoomToExtent(): void {
    // Si une couche est sélectionnée et visible, zoomer dessus
    if (this.selectedLayer?.visible && this.selectedLayer.bbox) {
      const extent = transformExtent(
        this.selectedLayer.bbox,
        'EPSG:4326',
        'EPSG:3857'
      );
      this.map.getView().fit(extent, { padding: [50, 50, 50, 50], duration: 500 });
      return;
    }
    
    // Sinon, zoomer sur l'étendue globale des données
    if (this.globalBbox) {
      this.zoomToDataExtent();
    }
  }

  selectLayer(layer: WmsLayer): void {
    this.selectedLayer = this.selectedLayer === layer ? null : layer;
  }

  zoomToLayer(layer: WmsLayer): void {
    if (layer.bbox) {
      const extent = transformExtent(
        layer.bbox,
        'EPSG:4326',
        'EPSG:3857'
      );
      this.map.getView().fit(extent, { padding: [50, 50, 50, 50], duration: 500 });
    } else {
      // Si pas de bbox, essayer de la récupérer via DescribeLayer ou utiliser l'étendue globale
      this.fetchLayerBbox(layer).then(bbox => {
        if (bbox) {
          layer.bbox = bbox;
          const extent = transformExtent(bbox, 'EPSG:4326', 'EPSG:3857');
          this.map.getView().fit(extent, { padding: [50, 50, 50, 50], duration: 500 });
        } else if (this.globalBbox) {
          // Fallback sur l'étendue globale
          this.zoomToDataExtent();
        }
      });
    }
  }

  private async fetchLayerBbox(layer: WmsLayer): Promise<[number, number, number, number] | null> {
    try {
      const capUrl = `${this.geoserverUrl}/${this.workspace}/wms?service=WMS&version=1.1.1&request=GetCapabilities`;
      const response = await fetch(capUrl);
      const xml = await response.text();
      const parser = new DOMParser();
      const doc = parser.parseFromString(xml, 'text/xml');
      
      // Chercher la couche spécifique
      const layers = doc.querySelectorAll('Layer > Layer');
      for (const layerEl of Array.from(layers)) {
        const name = layerEl.querySelector('Name')?.textContent;
        if (name === layer.name) {
          const latLonBbox = layerEl.querySelector('LatLonBoundingBox');
          if (latLonBbox) {
            return [
              parseFloat(latLonBbox.getAttribute('minx') || '0'),
              parseFloat(latLonBbox.getAttribute('miny') || '0'),
              parseFloat(latLonBbox.getAttribute('maxx') || '0'),
              parseFloat(latLonBbox.getAttribute('maxy') || '0')
            ];
          }
        }
      }
    } catch (err) {
      console.error('Erreur lors de la récupération de la bbox:', err);
    }
    return null;
  }

  refreshLayers(): void {
    this.availableLayers.forEach(layer => {
      if (layer.layer && layer.visible) {
        const source = layer.layer.getSource();
        if (source) {
          source.refresh();
        }
      }
    });
  }

  updateGeoserverConfig(): void {
    // Recharger les couches avec la nouvelle configuration
    this.availableLayers.forEach(layer => {
      if (layer.layer) {
        this.map.removeLayer(layer.layer);
        layer.layer = undefined;
        layer.visible = false;
      }
    });
    this.loadAvailableLayers();
  }

  toggleLayerPanel(): void {
    this.showLayerPanel = !this.showLayerPanel;
  }

  getVisibleLayersCount(): number {
    return this.availableLayers.filter(l => l.visible).length;
  }
}
