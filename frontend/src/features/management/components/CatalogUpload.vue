<script setup lang="ts">
import { ref } from 'vue';
import { useToast } from "primevue/usetoast";
import FileUpload, { type FileUploadUploaderEvent } from 'primevue/fileupload';
// @ts-ignore
import type { Worksheet, Row, Cell } from 'exceljs';
// @ts-ignore
import * as ExcelJS from 'exceljs';

interface SearchHeaderResult {
  headerRowNumber: number;
  actualHeaders: string[];
}

const toast = useToast();
const isLoading = ref(false);

/**
 * Objeto de configuração para especificar o que deve ser ignorado em cada coluna
 */
const ignoreRules = {
  'Código do Grupo': [
    '29', // ACESSÓRIOS DE MOTORES
    '15', // AERONAVES E SEUS COMPONENTES ESTRUTURAIS
    '88', // ANIMAIS VIVOS
    '10', // ARMAMENTO
    '25', // COMPONENTES DE VEÍCULOS
    '16', // COMPONENTES E ACESSORIOS DE AERONAVES
    '23', // VEÍCULOS
    '24', // TRATORES
    '13', // MUNIÇÕES E EXPLOSIVOS
    '19', // NAVIOS, PEQUENAS EMBARCACOES, PONTOES E DIQUES FLUTUANTES
  ]
};

/**
 * Verifica se uma linha deve ser ignorada com base em um conjunto de regras.
 * @param rowObject O objeto representando a linha da planilha.
 * @param rules O objeto de configuração com as regras para ignorar.
 * @returns `true` se a linha deve ser ignorada, `false` caso contrário.
 */
const shouldIgnoreRow = (rowObject: { [key: string]: any }, rules: { [key: string]: string[] }): boolean => {
  // Itera sobre cada regra definida (ex: 'Status', 'Categoria')
  for (const columnName in rules) {
    if (rowObject.hasOwnProperty(columnName)) {
      const valuesToIgnore = rules[columnName].map(v => v.toLowerCase());
      const cellValue = String(rowObject[columnName] || '').toLowerCase();

      // Se o valor da célula estiver na lista de valores a serem ignorados para aquela coluna, retorne true.
      if (valuesToIgnore.includes(cellValue)) {
        return true;
      }
    }
  }
  // Se nenhuma regra correspondeu, a linha não deve ser ignorada.
  return false;
};

/**
 * Procura por uma linha de cabeçalho em uma planilha.
 * @param worksheet A planilha do ExcelJS a ser pesquisada.
 * @returns Um objeto contendo o número da linha do cabeçalho e os nomes das colunas.
 */
const searchFileHeader = (worksheet: Worksheet): SearchHeaderResult => {
  const headerKeywords = ['Nome', 'Código', 'Descrição'];
  let headerRowNumber = -1;
  let actualHeaders: string[] = [];

  for (let i = 1; i <= 10 && i <= worksheet.rowCount; i++) {
    const row: Row = worksheet.getRow(i);
    const rowValues: string[] = (row.values as string[] || []).map(v => String(v || '').toLowerCase());

    const hasAtLeastOneHeader = headerKeywords.some(keyword =>
      rowValues.some(cellValue => cellValue.includes(keyword.toLowerCase()))
    );

    if (hasAtLeastOneHeader) {
      headerRowNumber = i;
      actualHeaders = (row.values as string[]).filter(v => v).map(h => h.trim());
      break;
    }
  }
  return { headerRowNumber, actualHeaders };
};

/**
 * Lida com o upload de arquivos, converte para JSON e envia para a API,
 * com uma lógica para ignorar linhas específicas.
 * @param event O evento de upload do PrimeVue.
 */
const handleFileUpload = async (event: FileUploadUploaderEvent) => {
  // 3. Normalize o 'event.files' para SEMPRE ser um array
  const files = Array.isArray(event.files) ? event.files : [event.files];

  if (files.length === 0) {
    toast.add({ severity: 'error', summary: 'Erro', detail: 'Nenhum arquivo selecionado.', life: 3000 });
    return;
  }

  const file = files[0];

  isLoading.value = true;

  toast.add({
    group: 'loading',
    severity: 'info',
    summary: 'Processando...',
    detail: 'Aguarde enquanto seu arquivo é processado.',
    closable: false
  });

  try {
    const buffer = await file.arrayBuffer();
    const workbook = new ExcelJS.Workbook();
    await workbook.xlsx.load(buffer);

    const worksheet = workbook.worksheets[0];
    const jsonData: any[] = []; // O tipo final dependerá das suas colunas

    const { headerRowNumber, actualHeaders } = searchFileHeader(worksheet);

    if (headerRowNumber === -1) {
      // A variável 'headerKeywords' não existe neste escopo, vamos usar a string diretamente.
      throw new Error(`Cabeçalho não identificado. Verifique se a planilha contém colunas como: Nome, Código, Descrição.`);
    }

    worksheet.eachRow({ includeEmpty: false }, (row: Row, rowNumber: number) => {
      if (rowNumber > headerRowNumber) {
        const rowObject: { [key: string]: any } = {};

        row.eachCell({ includeEmpty: true }, (cell: Cell, colNumber: number) => {
          const header = actualHeaders[colNumber - 1];
          if (header) {
            rowObject[header] = cell.value;
          }
        });

        if (shouldIgnoreRow(rowObject, ignoreRules)) {
          console.log(`Linha ${rowNumber} ignorada devido a uma regra de filtro.`, rowObject);
          return; // Pula para a próxima linha
        }

        jsonData.push(rowObject);
      }
    });

    if (jsonData.length === 0) {
      throw new Error("Nenhum dado válido encontrado abaixo do cabeçalho.");
    }

    console.log('Dados extraídos de forma robusta:', jsonData);

    toast.add({
      severity: 'success',
      summary: 'Sucesso!',
      detail: 'Catálogo importado com sucesso.',
      life: 3000
    });

  } catch (error: any) {
    console.error('Erro no processo de upload:', error);
    toast.add({
      severity: 'error',
      summary: 'Falha na Importação',
      detail: error.message || 'Não foi possível processar o arquivo.',
      life: 5000
    });
  } finally {
    isLoading.value = false;
    toast.removeGroup('loading');
  }
};


</script>

<template>

  <FileUpload mode="basic" name="catalog-upload" @uploader="handleFileUpload" :customUpload="true"
    accept=".csv, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel"
    chooseLabel="Importar" chooseIcon="pi pi-upload" :maxFileSize="20000000" :auto="true" :disabled="isLoading"
    cancelLabel="Cancelar" uploadLabel="Carregar" class="text-sm" style="padding: 6px 10px;">
  </FileUpload>

</template>

<style scoped></style>