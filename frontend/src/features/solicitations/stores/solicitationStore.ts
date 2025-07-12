import { defineStore } from 'pinia';
import type { ItemCatalogo } from '@/types/itemsCatalogo'; // Reutilize sua interface de Item

// Defina a "forma" de um item dentro da solicitação (pode ter quantidade)
interface SolicitationItem extends ItemCatalogo {
  quantity: number;
  price: number;
}

export const useSolicitationStore = defineStore('solicitation', {
  // STATE: Onde os dados vivem
  state: () => ({
    items: [] as SolicitationItem[],
    justification: '',
    isLoading: false,
  }),

  // GETTERS: Propriedades computadas, como totais.
  getters: {
    totalItems: (state) => state.items.length,
    totalPrice: (state) => {
      return state.items.reduce((total, item) => total + (item.price * item.quantity), 0);
    },
    isCartEmpty: (state) => state.items.length === 0,
  },

  // ACTIONS: Funções que modificam o estado.
  actions: {
    addItem(itemToAdd: ItemCatalogo) {
      const existingItem = this.items.find((item) => item.id === itemToAdd.id);

      if (existingItem) {
        existingItem.quantity++;
      } else {
        this.items.push({ ...itemToAdd, quantity: 1, price: itemToAdd.suggestedUnitPrice});
      }
    },

    removeItem(itemId: string) {
      this.items = this.items.filter((item) => item.id !== itemId);
    },

    updateQuantity(itemId: string, quantity: number) {
      const item = this.items.find((item) => item.id === itemId);
      if (item) {
        if (quantity > 0) {
          item.quantity = quantity;
        } else {
          this.removeItem(itemId); // Remove o item se a quantidade for zero ou menos
        }
      }
    },

    setJustification(text: string) {
      this.justification = text;
    },

    async submitSolicitation() {
      this.isLoading = true;
      try {
        // Aqui você faria a chamada de API (ex: com Axios) para enviar os dados
        // const response = await api.post('/solicitations', {
        //   items: this.items,
        //   justification: this.justification,
        // });
        console.log('Enviando solicitação:', {
          items: this.items,
          justification: this.justification
        });
        this.clearSolicitation(); // Limpa o carrinho após o sucesso
      } catch (error) {
        console.error('Falha ao enviar solicitação:', error);
        // Tratar o erro (ex: mostrar um toast de erro)
      } finally {
        this.isLoading = false;
      }
    },
    
    clearSolicitation() {
        this.items = [];
        this.justification = '';
    }
  },
});