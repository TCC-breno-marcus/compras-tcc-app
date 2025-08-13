import { ref, onMounted, onUnmounted, computed } from 'vue';

const lgBreakpoint = 992; // Padrão do PrimeVue/Bootstrap para 'lg'

export function useBreakpoint() {
  const screenWidth = ref(window.innerWidth);

  const onResize = () => {
    screenWidth.value = window.innerWidth;
  };

  onMounted(() => window.addEventListener('resize', onResize));
  onUnmounted(() => window.removeEventListener('resize', onResize));

  const isLargeScreen = computed(() => screenWidth.value >= lgBreakpoint);

  return { isLargeScreen };
}