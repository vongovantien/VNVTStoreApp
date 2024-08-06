import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
@Component({
  selector: 'app-app-loader',
  standalone: true,
  imports:[MatProgressSpinnerModule],
  templateUrl: './app-loader.component.html',
  styleUrls: ['./app-loader.component.scss']
})
export class AppLoaderComponent implements OnInit {
  title = "";
  message = "";
  constructor(public dialogRef: MatDialogRef<AppLoaderComponent>) { }

  ngOnInit() {
  }

}
