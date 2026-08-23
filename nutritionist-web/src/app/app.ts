import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from './services/api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly api = inject(ApiService);

  name = '';
  age: number | null = null;
  weight: number | null = null;
  height: number | null = null;
  goal = 'Weight Loss';

  bmi: number | null = null;
  recommendation = '';
  apiStatus = 'Checking...';

  ngOnInit() {
    this.api.healthCheck().subscribe({
      next: () => this.apiStatus = 'API Connected',
      error: () => this.apiStatus = 'API Offline'
    });
  }

  calculate() {
    if (!this.weight || !this.height) return;

    const heightInMeters = this.height / 100;
    this.bmi = Number(
      (this.weight / (heightInMeters * heightInMeters)).toFixed(1)
    );

    if (this.bmi < 18.5) {
      this.recommendation = 'Focus on a nutrient-dense, calorie-surplus diet with adequate protein.';
    } else if (this.bmi < 25) {
      this.recommendation = 'Maintain a balanced high-protein diet with regular physical activity.';
    } else if (this.bmi < 30) {
      this.recommendation = 'Focus on controlled calories, high protein, vegetables and consistent activity.';
    } else {
      this.recommendation = 'Focus on sustainable calorie control, high protein, fibre and regular activity.';
    }
  }
}
