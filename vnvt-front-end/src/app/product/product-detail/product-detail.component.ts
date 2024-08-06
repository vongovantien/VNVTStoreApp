import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Comment, Product } from '../../core/models';
import { ProductService } from '../../core/services';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule,
    MatFormFieldModule,
    FormsModule,
    MatOptionModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss']
})
export class ProductDetailComponent implements OnInit {
  product!: Product;
  comments: Comment[] = [];
  trendingProducts: Product[] = [];
  newComment = {
    id: new Date().getTime(),
    user: 'John Doe',
    avatarUrl: 'https://via.placeholder.com/40',
    text: 'Great product!',
    rating: 5,
    replies: []
  };

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    //this.loadProductDetails();
    this.loadTrendingProducts();
  }

  loadProductDetails(): void {
    // Load product details here
    // this.productService.getProductById(1).subscribe(product => {
    //   this.product = product;
    //   this.comments = [];
    // });
  }

  loadTrendingProducts(): void {
    // Load trending products here
    this.productService.getTrendingProducts().subscribe(products => {
      this.trendingProducts = products;
    });
  }

  addToCart(): void {
    // Add to cart logic here
  }

  submitComment(): void {
    // Submit comment logic here
    this.comments.push(this.newComment);
    this.newComment = {
      id: new Date().getTime(),
      user: 'John Doe',
      avatarUrl: 'https://via.placeholder.com/40',
      text: 'Great product!',
      rating: 5,
      replies: []
    };
  }

  toggleReplyForm(commentId: number): void {
    console.log(commentId)
    const comment = this.comments.find(c => c.id === commentId);
    if (comment) {
      comment.showReplyForm = !comment.showReplyForm;
    }
  }

  submitReply(commentId: number): void {
    const comment = this.comments.find(c => c.id === commentId);
    if (comment && comment.replyText) {
      const newReply: Comment = {
        id: new Date().getTime(),
        user: 'Your Name', // Change to the current user's name
        avatarUrl: 'https://via.placeholder.com/40', // Change to the current user's avatar
        text: comment.replyText,
        rating: 0,
        replies: []
      };
      comment.replies?.push(newReply);
      comment.replyText = '';
      comment.showReplyForm = false;
    }
  }

  addComment(): void {
    if (this.newComment.text) {
      const newComment: Comment = {
        id: new Date().getTime(),
        user: 'Your Name', // Change to the current user's name
        avatarUrl: 'https://via.placeholder.com/40', // Change to the current user's avatar
        text: this.newComment.text,
        rating: 0,
        replies: []
      };
      this.comments.push(newComment);
      this.newComment.text = '';
    }
  }
}
