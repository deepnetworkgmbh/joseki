module.exports = {
  root: true,
  env: {
    node: true
  },
  'extends': [
    'plugin:vue/essential',
    'eslint:recommended',
    '@vue/typescript'
  ],
  rules: {
    /* TODO: get rid of console.log() in sources 'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',*/
    'no-console': 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'css.lint.emptyRules': 'off',
    'scss.lint.emptyRules': 'off',
    'less.lint.emptyRules': 'off',
    'prefer-const': 'off'
  },
  parserOptions: {
    parser: '@typescript-eslint/parser'
  }
}
