/**
 * Product Model
 * Represents a product in the e-commerce system.
 */
export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  sku: string;
  category: string;
  isActive: boolean;
}

/**
 * Product API Response - Paginated Result
 */
export interface PagedProductResult {
  items: Product[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Inventory Check Result
 */
export interface InventoryCheck {
  productId: string;
  requestedQuantity: number;
  availableQuantity: number;
  isAvailable: boolean;
}

/**
 * Order Model
 */
export interface Order {
  id: string;
  customerId: string;
  status: 'Pending' | 'Confirmed' | 'Shipped' | 'Completed' | 'Canceled' | 'Failed';
  totalAmount: number;
  createdAt: string;
  completedAt?: Date;
  lineItems: OrderLineItem[];
}

/**
 * Order Line Item
 */
export interface OrderLineItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

/**
 * Create Order Request
 */
export interface CreateOrderRequest {
  customerId: string;
  lineItems: OrderLineItem[];
}

/**
 * Shopping Cart Item
 */
export interface CartItem {
  product: Product;
  quantity: number;
  totalPrice: number;
}

/**
 * Shopping Cart
 */
export interface ShoppingCart {
  items: CartItem[];
  totalItems: number;
  totalPrice: number;
}
