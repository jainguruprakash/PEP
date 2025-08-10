import { Component, Input, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="chart-container">
      <canvas #chartCanvas></canvas>
    </div>
  `,
  styles: [`
    .chart-container {
      position: relative;
      width: 100%;
      height: 100%;
    }
    canvas {
      max-width: 100%;
      max-height: 100%;
    }
  `]
})
export class ChartComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chartCanvas', { static: true }) chartCanvas!: ElementRef<HTMLCanvasElement>;
  
  @Input() type: ChartType = 'line';
  @Input() data: any;
  @Input() options: any = {};
  @Input() height: number = 400;
  @Input() width?: number;

  private chart?: Chart;

  ngOnInit() {
    // Component initialization
  }

  ngAfterViewInit() {
    this.createChart();
  }

  ngOnDestroy() {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  private createChart() {
    if (!this.chartCanvas || !this.data) return;

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    // Set canvas dimensions
    this.chartCanvas.nativeElement.height = this.height;
    if (this.width) {
      this.chartCanvas.nativeElement.width = this.width;
    }

    const defaultOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top' as const,
        },
        tooltip: {
          mode: 'index' as const,
          intersect: false,
        },
      },
      scales: this.getScalesConfig(),
      interaction: {
        mode: 'nearest' as const,
        axis: 'x' as const,
        intersect: false,
      },
    };

    const config: ChartConfiguration = {
      type: this.type,
      data: this.data,
      options: { ...defaultOptions, ...this.options }
    };

    this.chart = new Chart(ctx, config);
  }

  private getScalesConfig() {
    switch (this.type) {
      case 'line':
      case 'bar':
        return {
          x: {
            display: true,
            grid: {
              display: true,
              color: 'rgba(0, 0, 0, 0.1)',
            },
          },
          y: {
            display: true,
            beginAtZero: true,
            grid: {
              display: true,
              color: 'rgba(0, 0, 0, 0.1)',
            },
          },
        };
      case 'pie':
      case 'doughnut':
        return {};
      default:
        return {
          x: {
            display: true,
          },
          y: {
            display: true,
            beginAtZero: true,
          },
        };
    }
  }

  updateChart(newData: any) {
    if (this.chart) {
      this.chart.data = newData;
      this.chart.update();
    }
  }

  updateOptions(newOptions: any) {
    if (this.chart) {
      this.chart.options = { ...this.chart.options, ...newOptions };
      this.chart.update();
    }
  }
}

// Chart data helper functions
export class ChartDataHelper {
  static createLineChartData(labels: string[], datasets: { label: string; data: number[]; borderColor?: string; backgroundColor?: string; }[]) {
    return {
      labels,
      datasets: datasets.map(dataset => ({
        label: dataset.label,
        data: dataset.data,
        borderColor: dataset.borderColor || '#3b82f6',
        backgroundColor: dataset.backgroundColor || 'rgba(59, 130, 246, 0.1)',
        borderWidth: 2,
        fill: true,
        tension: 0.4,
      }))
    };
  }

  static createBarChartData(labels: string[], datasets: { label: string; data: number[]; backgroundColor?: string[]; }[]) {
    return {
      labels,
      datasets: datasets.map(dataset => ({
        label: dataset.label,
        data: dataset.data,
        backgroundColor: dataset.backgroundColor || [
          '#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6', '#06b6d4'
        ],
        borderWidth: 1,
      }))
    };
  }

  static createPieChartData(labels: string[], data: number[], backgroundColor?: string[]) {
    return {
      labels,
      datasets: [{
        data,
        backgroundColor: backgroundColor || [
          '#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6', '#06b6d4',
          '#ec4899', '#84cc16', '#f97316', '#6366f1'
        ],
        borderWidth: 2,
        borderColor: '#ffffff',
      }]
    };
  }

  static createDoughnutChartData(labels: string[], data: number[], backgroundColor?: string[]) {
    return this.createPieChartData(labels, data, backgroundColor);
  }

  static getDefaultColors() {
    return {
      primary: '#3b82f6',
      success: '#10b981',
      warning: '#f59e0b',
      danger: '#ef4444',
      info: '#06b6d4',
      purple: '#8b5cf6',
      pink: '#ec4899',
      lime: '#84cc16',
      orange: '#f97316',
      indigo: '#6366f1',
    };
  }
}
