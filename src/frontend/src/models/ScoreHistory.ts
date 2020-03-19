import { DateTime } from 'luxon';

/// Represents the score of component at a given date.
export class ScoreHistoryItem {
  constructor(public recordedAt: string, public score: number) { }
}
