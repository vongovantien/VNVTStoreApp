// src/app/models/paging-parameters.ts
export interface PagingParameters {
  PageNumber: number;
  PageSize: number;
  Keyword?: string;
  SortField?: string;
  SortDescending: boolean;
}
