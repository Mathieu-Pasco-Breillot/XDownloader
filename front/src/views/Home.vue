<template>
  <div class="home">
    <img src="../assets/logo.png" class="center"/>
    <Download/>
  </div>
</template>

<script lang="ts">
  import Download           from '@/components/Download.vue'; // @ is an alias to /src
  import { Component, Vue } from 'vue-property-decorator';

  @Component( {
    components: {
      Download,
    },
  } )
  export default class Home extends Vue {
  }
</script>
