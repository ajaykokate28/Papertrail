# Evaluation Plan

The assignment requires ten questions covering the five source papers and conversational recall. The current application provides the generation and history contracts; ingestion, embeddings, vector retrieval, and four-turn memory are tracked as future implementation work.

## Question set

1. What problem does the Transformer architecture solve compared with recurrent processing?
2. How does scaled dot-product attention calculate its output?
3. Why does the Transformer use multiple attention heads?
4. What is BERT's bidirectional pre-training objective?
5. How does masked language modeling support BERT's representations?
6. What is the purpose of next sentence prediction in BERT?
7. What distinguishes GPT-2's training setup from task-specific supervised training?
8. How does GPT pre-training transfer to downstream tasks?
9. What is XLNet's permutation language modeling objective?
10. Given a previous answer, can the assistant explain how the cited method relates to the earlier comparison?

## Scoring rubric

Score each answer from 0 to 4 for each dimension:

- Relevance: directly addresses the question.
- Accuracy: agrees with the cited source passage.
- Contextual awareness: uses the recent conversational context when applicable.
- Response quality: clear, concise, and technically understandable.

The maximum score is 16 per question and 160 for the complete set. Record the retrieved chunks, model, prompt, response, and score for every run. Do not report evaluation results until the ingestion, retrieval, and memory components are active.
