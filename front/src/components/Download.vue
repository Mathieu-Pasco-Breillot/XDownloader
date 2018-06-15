<template>
  <div>
    <v-layout row wrap>
      <v-flex xs12 md5>
        <v-text-field v-model="protectorUrl" label="Protector Url"/>
        <v-btn @click.prevent="getLinksFromProtector">OK</v-btn>
      </v-flex>

      <v-flex xs12 md5 offset-md2>
        <v-text-field v-model="sourceUrl" label="Source Url" disabled/>
        <v-btn @click.prevent="getLinksFromSource" disabled>OK</v-btn>
      </v-flex>

    </v-layout>
    <v-flex>
      <v-progress-circular
          :size="100"
          :width="15"
          :value="progressValue"
          :rotate="180"
          color="primary"
      />
    </v-flex>
  </div>
</template>

<script lang="ts">
  import { Component, Vue } from "vue-property-decorator";
  import axios              from "axios";

  @Component
  export default class HelloWorld extends Vue {
    // initial data
    protectorUrl: string = "";
    sourceUrl: string    = "";
    interval             = {};
    progressValue        = 0;

    // lifecycle hook
    mounted() {
      this.interval = setInterval( () => {
        if (this.progressValue === 100) {
          return (this.progressValue = 0);
        }
        this.progressValue += 10;
      }, 1000 );
    }

    // computed
    // get progressValue(): number {
    //   return 0;
    // }

    // methods
    getLinksFromProtector() {
      // Starts to disply progress

      axios.post( "http://localhost:56254/api/LinksFromProtector", {
        // TODO: Pass the protector url.
      } ).then( function (response) {
        console.log( response );
      } ).catch( function (error) {
        console.log( error );
      } );
    }

    protected getLinksFromSource() {
      axios.post( "http://localhost:56254/api/LinksFromSource", {
        // TODO: Pass the source url.
      } ).then( function (response) {
        console.log( response );
      } ).catch( function (error) {
        console.log( error );
      } );
    }
  }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
  .grid {
    display: grid;
    grid-template-columns: auto auto;
  }
</style>
