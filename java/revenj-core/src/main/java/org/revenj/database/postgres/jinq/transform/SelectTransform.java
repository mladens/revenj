package org.revenj.database.postgres.jinq.transform;

import ch.epfl.labos.iu.orm.queryll2.symbolic.TypedValueVisitorException;
import org.revenj.database.postgres.jinq.jpqlquery.JinqPostgresQuery;
import org.revenj.database.postgres.jinq.jpqlquery.SelectOnly;
import org.revenj.database.postgres.jinq.jpqlquery.ColumnExpressions;
import org.revenj.database.postgres.jinq.jpqlquery.SelectFromWhere;

public class SelectTransform extends RevenjOneLambdaQueryTransform {
	boolean withSource;

	public SelectTransform(RevenjQueryTransformConfiguration config, boolean withSource) {
		super(config);
		this.withSource = withSource;
	}

	public <U, V> JinqPostgresQuery<U> apply(JinqPostgresQuery<V> query, LambdaAnalysis lambda, SymbExArgumentHandler parentArgumentScope) throws QueryTransformException {
		try {
			if (query.isSelectFromWhere() || query.isSelectFromWhereGroupHaving()) {
				SelectFromWhere<V> sfw = (SelectFromWhere<V>) query;
				SelectFromWhereLambdaArgumentHandler argHandler = SelectFromWhereLambdaArgumentHandler.fromSelectFromWhere(sfw, lambda, config.metamodel, parentArgumentScope, withSource);
				SymbExToColumns translator = config.newSymbExToColumns(argHandler, lambda.getLambdaIndex());

				ColumnExpressions<U> returnExpr = makeSelectExpression(translator, lambda);

				// Create the new query, merging in the analysis of the method
				SelectFromWhere<U> toReturn = (SelectFromWhere<U>) sfw.shallowCopy();
				// TODO: translator.transform() should return multiple columns, not just one thing
				toReturn.cols = returnExpr;
				return toReturn;
			} else if (query.isSelectOnly()) {
				SelectOnly<V> sfw = (SelectOnly<V>) query;
				SelectFromWhereLambdaArgumentHandler argHandler = SelectFromWhereLambdaArgumentHandler.fromSelectOnly(sfw, lambda, config.metamodel, parentArgumentScope, false);
				SymbExToColumns translator = config.newSymbExToColumns(argHandler, lambda.getLambdaIndex());

				ColumnExpressions<U> returnExpr = makeSelectExpression(translator, lambda);

				// Create the new query, merging in the analysis of the method
				SelectOnly<U> toReturn = (SelectOnly<U>) sfw.shallowCopy();
				// TODO: translator.transform() should return multiple columns, not just one thing
				toReturn.cols = returnExpr;
				return toReturn;
			}
			throw new QueryTransformException("Existing query cannot be transformed further");
		} catch (TypedValueVisitorException e) {
			throw new QueryTransformException(e);
		}
	}

	@Override
	public String getTransformationTypeCachingTag() {
		return SelectTransform.class.getName();
	}
}
