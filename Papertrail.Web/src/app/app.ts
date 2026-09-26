import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface GenerateResponse {
  requestId: string;
  status: string;
  answer: string;
  generatedAt: string;
}

interface HistoryItem {
  requestId: string;
  conversationId: string | null;
  prompt: string;
  answer: string;
  createdAt: string;
}

@Component({
  selector: 'app-root',
  imports: [DatePipe, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly http = inject(HttpClient);
  protected readonly prompt = signal('');
  protected readonly context = signal('');
  protected readonly conversationId = signal('research-session');
  protected readonly answer = signal<GenerateResponse | null>(null);
  protected readonly history = signal<HistoryItem[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isLoadingHistory = signal(false);
  protected readonly error = signal('');
  protected readonly apiOnline = signal<boolean | null>(null);

  constructor() {
    this.checkHealth();
    this.loadHistory();
  }

  protected submit(): void {
    const value = this.prompt().trim();
    if (!value || this.isLoading()) return;

    this.isLoading.set(true);
    this.error.set('');
    this.answer.set(null);
    this.http.post<GenerateResponse>('/api/ai/generate', {
      prompt: value,
      conversationId: this.conversationId().trim() || 'research-session',
      context: this.context().trim() || null
    }).subscribe({
      next: response => {
        this.answer.set(response);
        this.prompt.set('');
        this.apiOnline.set(true);
        this.isLoading.set(false);
        this.loadHistory();
      },
      error: (response: HttpErrorResponse) => {
        this.error.set(response.error?.detail ?? 'The assistant could not complete that request.');
        this.apiOnline.set(response.status !== 0);
        this.isLoading.set(false);
      }
    });
  }

  protected loadHistory(): void {
    this.isLoadingHistory.set(true);
    this.http.get<HistoryItem[]>('/api/ai/history', {
      params: { conversationId: this.conversationId(), limit: 20 }
    }).subscribe({
      next: items => {
        this.history.set(items);
        this.isLoadingHistory.set(false);
      },
      error: () => this.isLoadingHistory.set(false)
    });
  }

  protected useHistory(item: HistoryItem): void {
    this.prompt.set(item.prompt);
    this.answer.set({ ...item, status: 'history', generatedAt: item.createdAt });
  }

  private checkHealth(): void {
    this.http.get('/health').subscribe({
      next: () => this.apiOnline.set(true),
      error: () => this.apiOnline.set(false)
    });
  }
}
