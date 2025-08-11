import { Component, Input, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';

declare var Chart: any;

@Component({
  selector: 'app-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="chart-container" [style.height.px]="height">
      <canvas #chartCanvas></canvas>
    </div>
  `,
  styles: [`
    .chart-container {
      position: relative;
      width: 100%;
    }
    canvas {
      max-width: 100%;
      max-height: 100%;
    }
  `]
})
export class ChartComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chartCanvas', { static: true }) chartCanvas!: ElementRef<HTMLCanvasElement>;
  
  @Input() type: 'line' | 'bar' | 'pie' | 'doughnut' | 'radar' | 'polarArea' = 'line';
  @Input() data: any;
  @Input() options: any = {};
  @Input() height: number = 400;

  private chart: any;

  ngOnInit() {
    // Load Chart.js if not already loaded
    if (typeof Chart === 'undefined') {
      this.loadChartJS();
    }
  }

  ngAfterViewInit() {
    if (typeof Chart !== 'undefined') {
      this.createChart();
    }
  }

  ngOnDestroy() {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  private loadChartJS() {
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/chart.js';
    script.onload = () => {
      this.createChart();
    };
    document.head.appendChild(script);
  }

  private createChart() {
    if (!this.data || typeof Chart === 'undefined') {
      return;
    }

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    // Destroy existing chart
    if (this.chart) {
      this.chart.destroy();
    }

    // Default options
    const defaultOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top' as const,
        },
      },
    };

    // Merge with provided options
    const chartOptions = { ...defaultOptions, ...this.options };

    this.chart = new Chart(ctx, {
      type: this.type,
      data: this.data,
      options: chartOptions
    });
  }

  updateChart(newData: any) {
    if (this.chart && newData) {
      this.chart.data = newData;
      this.chart.update();
    }
  }
}

// Helper class for creating chart data
export class ChartDataHelper {
  static createLineChartData(labels: string[], datasets: any[]) {
    return {
      labels,
      datasets: datasets.map(dataset => ({
        ...dataset,
        fill: false,
        tension: 0.1
      }))
    };
  }

  static createBarChartData(labels: string[], datasets: any[]) {
    return {
      labels,
      datasets
    };
  }

  static createPieChartData(labels: string[], data: number[]) {
    return {
      labels,
      datasets: [{
        data,
        backgroundColor: this.getDefaultColors().palette,
        borderWidth: 1
      }]
    };
  }

  static createDoughnutChartData(labels: string[], data: number[]) {
    return {
      labels,
      datasets: [{
        data,
        backgroundColor: this.getDefaultColors().palette,
        borderWidth: 2,
        borderColor: '#fff'
      }]
    };
  }

  static getDefaultColors() {
    return {
      primary: '#1976d2',
      secondary: '#dc004e',
      success: '#10b981',
      warning: '#f59e0b',
      danger: '#ef4444',
      info: '#06b6d4',
      palette: [
        '#1976d2', '#dc004e', '#10b981', '#f59e0b', 
        '#ef4444', '#06b6d4', '#8b5cf6', '#f97316',
        '#84cc16', '#ec4899', '#6366f1', '#14b8a6'
      ]
    };
  }

  static generateSampleLineData(labels: string[], datasetLabel: string = 'Sample Data') {
    return this.createLineChartData(labels, [{
      label: datasetLabel,
      data: labels.map(() => Math.floor(Math.random() * 100)),
      borderColor: this.getDefaultColors().primary,
      backgroundColor: 'rgba(25, 118, 210, 0.1)'
    }]);
  }

  static generateSampleBarData(labels: string[], datasetLabel: string = 'Sample Data') {
    return this.createBarChartData(labels, [{
      label: datasetLabel,
      data: labels.map(() => Math.floor(Math.random() * 100)),
      backgroundColor: this.getDefaultColors().primary
    }]);
  }

  static generateSamplePieData(labels: string[]) {
    return this.createPieChartData(labels, labels.map(() => Math.floor(Math.random() * 100)));
  }
}