package org.revenj.database.postgres.jinq.jpqlquery;

public class GroupedSelectFromWhere<T, U> extends SelectFromWhere<T> {
	public ColumnExpressions<U> groupingCols;
	public Expression having;

	@Override
	protected void prepareQueryGeneration(
			Expression.QueryGenerationPreparationPhase preparePhase,
			QueryGenerationState queryState) {
		super.prepareQueryGeneration(preparePhase, queryState);
		for (Expression col : groupingCols.columns)
			col.prepareQueryGeneration(preparePhase, queryState);
		if (having != null)
			having.prepareQueryGeneration(preparePhase, queryState);
	}

	protected String generateQueryContents(QueryGenerationState queryState) {
		generateSelectFromWhere(queryState);
		generateGroupBy(queryState);
		generateSort(queryState);
		return queryState.buildQueryString();
	}

	protected void generateGroupBy(QueryGenerationState queryState) {
		queryState.appendQuery(" GROUP BY ");
		boolean isFirst = true;
		for (Expression col : groupingCols.columns) {
			if (!isFirst) queryState.appendQuery(", ");
			isFirst = false;
			col.generateQuery(queryState, OperatorPrecedenceLevel.JPQL_UNRESTRICTED_OPERATOR_PRECEDENCE);
		}
		if (having != null) {
			queryState.appendQuery(" HAVING ");
			having.generateQuery(queryState, OperatorPrecedenceLevel.JPQL_UNRESTRICTED_OPERATOR_PRECEDENCE);
		}
	}

	@Override
	public boolean isSelectFromWhere() {
		return false;
	}

	@Override
	public boolean isSelectFromWhereGroupHaving() {
		return sort.isEmpty() && limit < 0 && skip < 0;
	}

	@Override
	public GroupedSelectFromWhere<T, U> shallowCopy() {
		GroupedSelectFromWhere<T, U> copy = new GroupedSelectFromWhere<>();
		copySelectFromWhereTo(copy);
		copy.groupingCols = groupingCols;
		copy.having = having;
		return copy;
	}
}
