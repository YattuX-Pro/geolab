import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpRequest, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface GpkgFile {
  id: string;
  fileName: string;
  uploadDate: string;
  description: string | null;
  layerCount: number;
  totalFeatureCount: number;
}

export interface GpkgFileDetail {
  id: string;
  fileName: string;
  uploadDate: string;
  description: string | null;
  layers: GpkgLayer[];
}

export interface GpkgLayer {
  id: string;
  layerName: string;
  geometryType: string;
  featureCount: number;
}

export interface GpkgUploadResponse {
  fileId: string;
  fileName: string;
  layerCount: number;
  totalFeatureCount: number;
  message: string;
}

export interface GpkgFeatureCollection {
  type: 'FeatureCollection';
  features: GpkgFeature[];
}

export interface GpkgFeatureCollectionWithMetadata {
  type: 'FeatureCollection';
  features: GpkgFeature[];
  metadata: {
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    hasBbox: boolean;
  };
}

export interface GpkgFeature {
  type: 'Feature';
  id: string;
  properties: {
    layerName: string;
    layerId: string;
    [key: string]: unknown;
  };
  geometry: GeoJsonGeometry;
}

export interface GeoJsonGeometry {
  type: string;
  coordinates: unknown;
}

export interface UploadProgress {
  progress: number;
  loaded: number;
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class GpkgService {
  private apiUrl = 'http://localhost:5000/api/gpkg';

  constructor(private http: HttpClient) {}

  uploadFile(file: File, description?: string): Observable<HttpEvent<GpkgUploadResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    if (description) {
      formData.append('description', description);
    }

    const req = new HttpRequest('POST', `${this.apiUrl}/upload`, formData, {
      reportProgress: true
    });

    return this.http.request<GpkgUploadResponse>(req);
  }

  uploadFileSimple(file: File, description?: string): Observable<GpkgUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    if (description) {
      formData.append('description', description);
    }

    return this.http.post<GpkgUploadResponse>(`${this.apiUrl}/upload`, formData);
  }

  getAllFiles(): Observable<GpkgFile[]> {
    return this.http.get<GpkgFile[]>(`${this.apiUrl}/files`);
  }

  getFileById(id: string): Observable<GpkgFileDetail> {
    return this.http.get<GpkgFileDetail>(`${this.apiUrl}/files/${id}`);
  }

  getFileLayers(id: string): Observable<GpkgLayer[]> {
    return this.http.get<GpkgLayer[]>(`${this.apiUrl}/files/${id}/layers`);
  }

  getFileFeatures(fileId: string): Observable<GpkgFeatureCollection> {
    return this.http.get<GpkgFeatureCollection>(`${this.apiUrl}/files/${fileId}/features`);
  }

  getFileFeaturesWithPagination(
    fileId: string, 
    page: number = 1, 
    pageSize: number = 1000,
    bbox?: { minLng: number; minLat: number; maxLng: number; maxLat: number }
  ): Observable<GpkgFeatureCollectionWithMetadata> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (bbox) {
      params = params
        .set('minLng', bbox.minLng.toString())
        .set('minLat', bbox.minLat.toString())
        .set('maxLng', bbox.maxLng.toString())
        .set('maxLat', bbox.maxLat.toString());
    }

    return this.http.get<GpkgFeatureCollectionWithMetadata>(
      `${this.apiUrl}/files/${fileId}/features`,
      { params }
    );
  }

  deleteFile(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/files/${id}`);
  }
}
