import { useMutation } from '@tanstack/react-query';
import { CheckoutRepository } from '../repositories/CheckoutRepository';
import { CustomerInfo, OrderItem } from '../types';

export const useCheckoutMutation = () => {
    // Direct instantiation or use a DI container if strict logic needed. 
    // For this pattern, mostly Repositories are singletons or stateless.
    const repository = new CheckoutRepository();

    return useMutation({
        mutationFn: async (variables: { customer: CustomerInfo; items: OrderItem[] }) => {
            return repository.submitOrder(variables.customer, variables.items);
        },
    });
};
