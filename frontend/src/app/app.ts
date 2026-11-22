import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

type Source = { filePath: string; className?: string; methodName?: string; startLine: number; endLine: number; score: number };
type SearchResult = Source & { content: string; chunkId: string };

@Component({ selector: 'app-root', imports: [FormsModule], templateUrl: './app.html', styleUrl: './app.scss' })
export class App {
  private readonly http = inject(HttpClient);
  repositoryId = localStorage.getItem('repositoryId') ?? '';
  question = '';
  searchQuery = '';
  selectedFile?: File;
  busy = signal('');
  message = signal('Upload a repository to get started.');
  answer = signal('');
  sources = signal<Source[]>([]);
  results = signal<SearchResult[]>([]);

  selectFile(event: Event) { this.selectedFile = (event.target as HTMLInputElement).files?.[0]; }

  upload() {
    if (!this.selectedFile) return this.message.set('Choose a ZIP file first.');
    const body = new FormData();
    body.append('file', this.selectedFile);
    this.run('upload', this.http.post<{ repositoryId: string }>('/api/repository/upload', body), data => {
      this.repositoryId = data.repositoryId;
      localStorage.setItem('repositoryId', data.repositoryId);
      this.message.set('Repository uploaded. Index it before searching.');
    });
  }

  index() {
    if (!this.validRepository()) return;
    this.run('index', this.http.post(`/api/repository/index/${this.repositoryId}`, {}), () =>
      this.message.set('Repository indexed and ready.'));
  }

  ask() {
    if (!this.validRepository() || !this.question.trim()) return;
    this.answer.set('');
    this.sources.set([]);
    this.run('ask', this.http.post<{ answer: string; sources: Source[] }>('/api/chat/query', {
      repositoryId: this.repositoryId, question: this.question, topK: 5
    }), data => { this.answer.set(data.answer); this.sources.set(data.sources); });
  }

  search() {
    if (!this.validRepository() || !this.searchQuery.trim()) return;
    this.results.set([]);
    this.run('search', this.http.post<{ results: SearchResult[] }>('/api/search', {
      repositoryId: this.repositoryId, query: this.searchQuery, topK: 8
    }), data => this.results.set(data.results));
  }

  shortId() { return this.repositoryId ? `${this.repositoryId.slice(0, 8)}…` : 'No repository'; }

  private validRepository() {
    if (this.repositoryId) return true;
    this.message.set('Upload a repository or enter its ID.');
    return false;
  }

  private run<T>(key: string, request: Observable<T>, done: (data: T) => void) {
    this.busy.set(key);
    request.subscribe({
      next: data => { done(data); this.busy.set(''); },
      error: (error: HttpErrorResponse) => {
        this.message.set(error.error?.detail ?? (typeof error.error === 'string' ? error.error : 'Request failed. Check the backend configuration.'));
        this.busy.set('');
      }
    });
  }
}
