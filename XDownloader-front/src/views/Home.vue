<template>
  <div class="home">
    <img src="../assets/logo.png">
    <DownloadForm/>
  </div>
</template>

<script lang="ts">
  import { Component, Vue } from 'vue-property-decorator';
  import DownloadForm from '@/components/DownloadForm.vue'; // @ is an alias to /src

  @Component({
    components: {
      DownloadForm,
    },
  })
  export default class Home extends Vue {
  }
</script>
