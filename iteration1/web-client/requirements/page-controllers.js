(function(global){
  'use strict';
  const controllers=new Map();
  function register(name,controller){if(name&&controller)controllers.set(name,controller);}
  function get(name){return controllers.get(name)||null;}
  global.FICCPageControllers=Object.freeze({register,get});
})(window);
