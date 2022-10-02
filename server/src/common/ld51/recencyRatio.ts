export function getRecencyRatio(el: {
  ratio?: number
  created?: number
}) {
  if (!el.ratio || !el.created) return 0
  const age = Date.now() - el.created
  const yearsAfterLaunch =
    age / 1000 / 60 / 60 / 24 / 365 - 2022
  return el.ratio + yearsAfterLaunch * 0.1
  // * adds .1 for every year after launch, causing older songs to be less likely to be highlighted
}
