import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);

  private apiUrl =
    'https://fantastic-carnival-77p7xq6j46w4h9rv-5150.app.github.dev';

  healthCheck(): Observable<any> {
    return this.http.get(`${this.apiUrl}/api/health`);
  }
}
