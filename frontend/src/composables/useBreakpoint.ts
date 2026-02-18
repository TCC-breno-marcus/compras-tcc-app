import { ref, onMounted, onUnmounted, computed } from 'vue';

const lgBreakpoint = 992; // Padrão do PrimeVue/Bootstrap para 'lg'

/**
 * Observa largura da tela e expõe estado reativo para layouts responsivos.
 * @returns Indicador se viewport atual está no breakpoint `lg` ou superior.
 */
export const useBreakpoint = () => {
  const screenWidth = ref(window.innerWidth);

  /**
   * Mantém a largura de tela sincronizada com o resize do navegador.
   */
  const onResize = () => {
    screenWidth.value = window.innerWidth;
  };

  onMounted(() => window.addEventListener('resize', onResize));
  onUnmounted(() => window.removeEventListener('resize', onResize));

  const isLargeScreen = computed(() => screenWidth.value >= lgBreakpoint);

  return { isLargeScreen };
}
