import { Component, OnInit } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
@Component({
  selector: 'app-app-loader',
  standalone: true,
  imports:[MatProgressSpinnerModule, MatDialogModule],
  templateUrl: './app-loader.component.html',
  styleUrls: ['./app-loader.component.scss']
})
export class AppLoaderComponent implements OnInit {
  title = "";
  message = "";
  constructor() { }

  ngOnInit() {
  }

}
