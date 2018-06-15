<template>
  <div class="home">
    <img src="../assets/logo.png">
    <Download/>
  </div>
</template>

<script lang="ts">
  import { Component, Vue } from "vue-property-decorator";
  import Download           from "@/components/Download.vue"; // @ is an alias to /src

  @Component( {
    components: {
      Download,
    },
  } )
  export default class Home extends Vue {
  }
</script>
